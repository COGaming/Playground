using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Playground
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch _spriteBatch;
        private Texture2D _player;
        private Vector2 _playerPosition;

        private float _floor = 350;
        private float _maxLeft = 0;
        private float _maxRight = 400;
        private float _roof = 0;
        private float _gravity = 100;
        private float _walkSpeed = 50;
        private float _fallSpeed = 70;
        private float _moveSpeed = 70;
        private float _jumpSpeed = 270;
        private bool _isFalling = false;
        private bool _isJumping = false;
        private float _maxJumpHeight = 100;
        private float _minJumpTime = 0.10f;
        private float _maxJumpTime = 0.35f;
        private float _currentJumpTime;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _player = new Texture2D(GraphicsDevice, 50, 50);

            var data = new Color[50 * 50];

            for (var i = 0; i < data.Length; ++i)
            {
                data[i] = Color.Chocolate;
            }

            _player.SetData(data);

            _playerPosition = Vector2.Zero;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
        }

        private void Move(Vector2 direction, GameTime gameTime)
        {
            var elapsedTotalSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _isJumping = 
                (direction.Y > 0 || (_currentJumpTime < _minJumpTime && _isJumping)) &&
                (_currentJumpTime <= _maxJumpTime);

            _isFalling = _playerPosition.Y < _floor && !_isJumping;

            Debug.WriteLine(_currentJumpTime);

            if (!_isJumping)
            {
                _currentJumpTime = 0f;
                _isFalling = true;
            }

            if (_isJumping)
            {
                _playerPosition.Y -= (_jumpSpeed * direction.Y) * elapsedTotalSeconds;
                _currentJumpTime += elapsedTotalSeconds;
            }

            if (_isFalling)
            {
                
            }

            _playerPosition.Y += _gravity * elapsedTotalSeconds;

            if (direction.X != 0.0f)
            {
                _playerPosition.X += (_moveSpeed * direction.X) * elapsedTotalSeconds;
            }

            _playerPosition.Y = MathHelper.Min(_floor, _playerPosition.Y);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            var direction = Vector2.Zero;

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                //_playerPosition.Y = MathHelper.Max(_roof, _playerPosition.Y - 1);
                direction.Y = 1;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                //_playerPosition.X = MathHelper.Min(_maxRight, _playerPosition.X + 1);

                direction.X = 1;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                //_playerPosition.X = MathHelper.Max(_maxLeft, _playerPosition.X - 1);

                direction.X = -1;
            }

            Move(direction, gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(
                sortMode: SpriteSortMode.BackToFront, 
                blendState: BlendState.AlphaBlend, 
                depthStencilState: DepthStencilState.Default
            );

            _spriteBatch.Draw(_player, _playerPosition, color: Color.Black);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
