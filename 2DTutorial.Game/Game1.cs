using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2DTutorial
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _device;
        private SpriteBatch _spriteBatch;

        private Texture2D _backgroundTexture;
        private Texture2D _foregroundTexture;


        private int _screenWidth;
        private int _screenHeight;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 500;
            _graphics.PreferredBackBufferHeight = 500;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();

            Window.Title = "2D Monogame Tutorial";

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _device = _graphics.GraphicsDevice;

            _screenWidth = _device.PresentationParameters.BackBufferWidth; 
            _screenHeight = _device.PresentationParameters.BackBufferHeight;

            _backgroundTexture = Content.Load<Texture2D>("background");
            _foregroundTexture = Content.Load<Texture2D>("foreground");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            DrawScenery();
            _spriteBatch.End(); 

            base.Draw(gameTime);
        }
    
        private void DrawScenery()
		{
            var screenRect = new Rectangle(0, 0, _screenWidth, _screenHeight);
            _spriteBatch.Draw(_backgroundTexture, screenRect, Color.White);
            _spriteBatch.Draw(_foregroundTexture, screenRect, Color.White);
		}
    }
}
