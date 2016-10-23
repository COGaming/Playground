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
        private MoveableObject _moveableObject;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _moveableObject = new MoveableObject();

            _moveableObject.Initialize();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _moveableObject.LoadContent(GraphicsDevice);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            //_moveableObject.UnloadContent();
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
                direction.Y = 1;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                direction.X = 1;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                direction.X = -1;
            }

            _moveableObject.Update(gameTime, direction);

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

            _moveableObject.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
