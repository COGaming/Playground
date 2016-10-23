using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Playground
{
    public class MoveableObject
    {
        // current map
        private const int MaxTop = 0;
        private const int MaxLeft = 0;
        private const int MaxBottom = 430;
        private const int MaxRight = 750;

        // constants
        private const float MoveSpeed = 0.5f;
        private const float JumpSpeed = 0.7f;
        private const float Gravity = 0.6f;
        private const float MaxHorizontalAcceleration = 1.0f;
        private const float MaxVerticalAcceleration = 2.4f;
        private const float MaxGravitalAcceleration = 18.0f;
        private const float MaxJumpTime = 350.0f;
        private const float GravitalAccelerationIncrement = 0.5f;
        private const float HorizontalAccelerationIncrement = 0.1f;
        private const float HorizontalAccelerationDecrement = 0.07f;
        private const float VerticalAccelerationDecrement = 0.05f;
        private const float GravitationalReduction = 1.5f;
        private const float MaxAllowedTimeToJumpAfterPush = 150f;
        private const float MinAllowedTimeBetweenJump = 75f;

        private bool _wasPushingTop;
        private bool _wasPushingBottom;
        private bool _wasPushingLeft;
        private bool _wasPushingRight;
        private float _currentHorizontalAcceleration;
        private float _currentVerticalAcceleration;
        private float _currentGravitalAcceleration;
        private float _currentJumpTime;
        private float _horizontalVelocity;
        private float _verticalVelocity;
        private float _gravitalVelocity;
        private float _timeSincePushedTop;
        private float _timeSincePushedBottom;
        private float _timeSincePushedLeft;
        private float _timeSincePushedRight;
        private float _timeSinceJump;
        private Vector2 _currentDirection;
        private Vector2 _currentPosition;
        private Texture2D _texture;

        public void Initialize()
        {
            _wasPushingTop = false;
            _wasPushingBottom = false;
            _wasPushingLeft = false;
            _wasPushingRight = false;
            _currentHorizontalAcceleration = 0;
            _currentVerticalAcceleration = 0;
            _currentGravitalAcceleration = 0;
            _currentJumpTime = 0;
            _horizontalVelocity = 0;
            _verticalVelocity = 0;
            _gravitalVelocity = 0;
            _timeSincePushedTop = MaxAllowedTimeToJumpAfterPush;
            _timeSincePushedBottom = MaxAllowedTimeToJumpAfterPush;
            _timeSincePushedLeft = MaxAllowedTimeToJumpAfterPush;
            _timeSincePushedRight = MaxAllowedTimeToJumpAfterPush;
            _timeSinceJump = MinAllowedTimeBetweenJump;
            _currentDirection = Vector2.Zero;
            _currentPosition = Vector2.Zero;
        }

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            _texture = new Texture2D(graphicsDevice, 50, 50);

            var data = new Color[50 * 50];

            for (var i = 0; i < data.Length; ++i)
            {
                data[i] = Color.Chocolate;
            }

            _texture.SetData(data);
        }

        private static bool IsOppositeHorizontalDirection(Vector2 a, Vector2 b)
        {
            if (a.X > 0 && b.X < 0)
            {
                return true;
            }

            if (a.X < 0 && b.X > 0)
            {
                return true;
            }

            return false;
        }

        private static CollisionResult GetCollisionResult(Vector2 position)
        {
            // retrieve these values from a collision response based on current position
            var isPushingTop = Math.Abs(position.Y) <= MaxTop;
            var isPushingBottom = Math.Abs(position.Y) >= MaxBottom;
            var isPushingLeft = Math.Abs(position.X) <= MaxLeft;
            var isPushingRight = Math.Abs(position.X) >= MaxRight;

            return new CollisionResult(
                isPushingTop, 
                isPushingBottom, 
                isPushingLeft, 
                isPushingRight
            );
        }

        private void UpdateHorizontalAcceleration(Vector2 direction)
        {
            var isOppositeDirection = IsOppositeHorizontalDirection(
                direction,
                _currentDirection
            );

            if (direction == Vector2.Zero)
            {
                // decrease horizontal acceleration
                _currentHorizontalAcceleration = MathHelper.Max(
                    0,
                    _currentHorizontalAcceleration - HorizontalAccelerationDecrement
                );
            }
            else if (isOppositeDirection)
            {
                // reset the current acceleration when the 
                // previous direction was in the opposite direction
                _currentHorizontalAcceleration = 0;
            }
            else
            {
                // increase horizontal acceleration
                _currentHorizontalAcceleration = MathHelper.Min(
                    MaxHorizontalAcceleration,
                    _currentHorizontalAcceleration + HorizontalAccelerationIncrement
                );
            }
        }

        private void UpdateVerticalAcceleration(Vector2 direction)
        {
            if (direction.Y > 0 && _currentJumpTime <= 0)
            {
                // considered as an initial jump, 
                // reset the vertical acceleration 
                // in order to give the jump a 
                // boost since the jump acceleration 
                // is decreasing rather than increasing
                _currentVerticalAcceleration = MaxVerticalAcceleration;
            }
            else if (direction.Y > 0)
            {
                // current direction still positive, so keep decreasing
                _currentVerticalAcceleration = MathHelper.Max(
                    0,
                    _currentVerticalAcceleration - VerticalAccelerationDecrement
                );
            }
            else
            {
                // current direction has no value or negative, no acceleration
                _currentVerticalAcceleration = 0;
            }
        }

        private void UpdateGravitalAcceleration(CollisionResult collisionResult)
        {
            if ((collisionResult.IsPushingLeft && !_wasPushingLeft) || (collisionResult.IsPushingRight && !_wasPushingRight))
            {
                // reset gravital acceleration in order to "stick" the entity slightly to the wall
                _currentGravitalAcceleration = 0;
            }

            // always increase the gravital acceleration
            _currentGravitalAcceleration = MathHelper.Min(
                MaxGravitalAcceleration,
                _currentGravitalAcceleration + GravitalAccelerationIncrement
            );
        }

        private void UpdateAccelerations(Vector2 direction, CollisionResult collisionResult)
        {
            UpdateHorizontalAcceleration(direction);

            UpdateVerticalAcceleration(direction);

            UpdateGravitalAcceleration(collisionResult);
        }

        private void UpdateVelocities(float elapsedMs)
        {
            _horizontalVelocity = _currentHorizontalAcceleration * elapsedMs;
            _verticalVelocity = _currentVerticalAcceleration * elapsedMs;
            _gravitalVelocity = Gravity * _currentGravitalAcceleration;
        }

        private float CalculateHorizontalMovement()
        {
            // determine movement along the X axis
            var x = _currentDirection.X * MoveSpeed * _horizontalVelocity;

            return x;
        }

        private bool IsAllowedToJump(CollisionResult collisionResult)
        {
            var jumpOnCooldown = _timeSinceJump < MinAllowedTimeBetweenJump;

            if (jumpOnCooldown)
            {
                return false;
            }

            var isCurrentlyPushing = 
                collisionResult.IsPushingTop || 
                collisionResult.IsPushingBottom ||
                collisionResult.IsPushingLeft || 
                collisionResult.IsPushingRight;

            if (isCurrentlyPushing)
            {
                return true;
            }

            var didPushWithinAllowedTime =
                _timeSincePushedTop <= MaxAllowedTimeToJumpAfterPush ||
                _timeSincePushedBottom <= MaxAllowedTimeToJumpAfterPush ||
                _timeSincePushedLeft <= MaxAllowedTimeToJumpAfterPush ||
                _timeSincePushedRight <= MaxAllowedTimeToJumpAfterPush;

            if (didPushWithinAllowedTime)
            {
                return true;
            }

            return false;
        }

        private float CalculateVerticalMovement(CollisionResult collisionResult)
        {
            var isAllowedToJump = IsAllowedToJump(collisionResult);

            // determine movement along the Y axis
            var y = _currentDirection.Y * JumpSpeed * _verticalVelocity;

            // restrict any movement in the Y axis when the jump has reached it's max time
            y = _currentJumpTime > MaxJumpTime
                ? 0
                : y;

            // restrict any movement in the Y axis if the jump isn't initial
            y = !isAllowedToJump && _currentJumpTime <= 0.0f
                ? 0
                : y;

            if (collisionResult.IsPushingLeft || collisionResult.IsPushingRight)
            {
                // reduce gravitational force when we're hugging a wall
                _gravitalVelocity /= GravitationalReduction;
            }

            // always apply gravitational force
            y -= _gravitalVelocity;

            return y;
        }

        private void UpdateTimers(float elapsedMs, float verticalMovement, CollisionResult collisionResult)
        {
            // increase our jump time
            _currentJumpTime = verticalMovement > 0.0f
                ? _currentJumpTime + elapsedMs
                : 0;

            // TODO:
            // - not sure what's best here, jumping between walls won't be possible using the collision restriction
            // - this technique will allow boosting a jump from ground by hitting a wall, but it will require the object
            // to hit the ground before being able to do another jump...

            // increase/reset the timer since last jump
            _timeSinceJump = Math.Abs(_currentJumpTime) <= 0 && collisionResult.IsPushingBottom
                ? _timeSinceJump + elapsedMs
                : 0;

            // update "bump" timers

            _timeSincePushedTop = collisionResult.IsPushingTop
                ? 0
                : _timeSincePushedTop + elapsedMs;

            _timeSincePushedBottom = collisionResult.IsPushingBottom
                ? 0
                : _timeSincePushedBottom + elapsedMs;

            _timeSincePushedLeft = collisionResult.IsPushingLeft
                ? 0
                : _timeSincePushedLeft + elapsedMs;

            _timeSincePushedRight = collisionResult.IsPushingRight
                ? 0
                : _timeSincePushedRight + elapsedMs;

            Debug.WriteLine(_timeSincePushedRight);
        }

        private void UpdateState(CollisionResult collisionResult)
        {
            // assign upon exit
            _wasPushingTop = collisionResult.IsPushingTop;
            _wasPushingBottom = collisionResult.IsPushingBottom;
            _wasPushingLeft = collisionResult.IsPushingLeft;
            _wasPushingRight = collisionResult.IsPushingRight;
        }

        public void Update(GameTime gameTime, Vector2 direction)
        {
            var elapsedMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (direction != Vector2.Zero)
            {
                // set latest known direction
                _currentDirection = direction;
            }

            var collisionResult = GetCollisionResult(_currentPosition);

            UpdateAccelerations(
                direction, 
                collisionResult
            );

            UpdateVelocities(elapsedMs);

            var horizontalMovement = CalculateHorizontalMovement();

            var verticalMovement = CalculateVerticalMovement(collisionResult);

            UpdateTimers(
                elapsedMs,
                verticalMovement, 
                collisionResult
            );
            
            // this should be an output rather than a straight assignment since we need to do further collision checking etc
            _currentPosition.X += horizontalMovement;
            _currentPosition.Y -= verticalMovement;

            UpdateState(collisionResult);

            // sanitize by checking if we're out of bounce
            // this will be replaced by checking if the new position collides
            // if the new position collides then an avoidance force should be applied
            // if the new position with avoidance force still collides the position should remain
            if (_currentPosition.Y > MaxBottom)
            {
                _currentPosition.Y = MaxBottom;
            }

            if (_currentPosition.Y <= MaxTop)
            {
                _currentPosition.Y = MaxTop;
            }

            if (_currentPosition.X < MaxLeft)
            {
                _currentPosition.X = MaxLeft;
            }

            if (_currentPosition.X >= MaxRight)
            {
                _currentPosition.X = MaxRight;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _currentPosition, color: Color.Black);
        }
    }
}
