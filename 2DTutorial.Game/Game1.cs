using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2DTutorial
{
    public struct PlayerData
    {
        public Vector2 Position;
        public bool IsAlive;
        public Color Color;
        public float Angle;
        public float Power;
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _device;
        private SpriteBatch _spriteBatch;

        private Texture2D _backgroundTexture;
        private Texture2D _foregroundTexture;
        private Texture2D _carriageTexture;
        private Texture2D _cannonTexture;
        
        private int _numPlayers = 4;
        private PlayerData[] _players;
        private float _playerScaling;

        private int _currentPlayer = 0;

        private SpriteFont _font;

        private Color[] _playerColors = new Color[10]
        {
             Color.Red,
             Color.Green,
             Color.Blue,
             Color.Purple,
             Color.Orange,
             Color.Indigo,
             Color.Yellow,
             Color.SaddleBrown,
             Color.Tomato,
             Color.Turquoise
        };

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

            SetUpPlayers();

            _backgroundTexture = Content.Load<Texture2D>("background");
            _foregroundTexture = Content.Load<Texture2D>("foreground");

            _carriageTexture = Content.Load<Texture2D>("carriage");
            _cannonTexture = Content.Load<Texture2D>("cannon");

            _font = Content.Load<SpriteFont>("myFont");

            // Since the width of each flat area on the terrain is 40 pixels, this scaling factor should scale the carriage so it fits on the flat area.
            _playerScaling = 40.0f / (float)_carriageTexture.Width;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            ProcessKeyboard();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            DrawScenery();
            DrawPlayers();
            DrawText();
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawScenery()
        {
            var screenRect = new Rectangle(0, 0, _screenWidth, _screenHeight);
            _spriteBatch.Draw(_backgroundTexture, screenRect, Color.White);
            _spriteBatch.Draw(_foregroundTexture, screenRect, Color.White);
        }

        private void DrawPlayers()
        {
            for(int i = 0; i < _players.Length; i++)
            {
                if (_players[i].IsAlive)
                {
                    int xPos = (int)_players[i].Position.X;
                    int yPos = (int)_players[i].Position.Y;
                    var cannonOrigin = new Vector2(11, 50);

                    _spriteBatch.Draw(
                        _carriageTexture, 
                        _players[i].Position, 
                        null, 
                        _players[i].Color, 
                        0, 
                        new Vector2(0, _carriageTexture.Height),
                        _playerScaling, 
                        SpriteEffects.None, 
                        0
                        );


                    _spriteBatch.Draw(
                        _cannonTexture, 
                        new Vector2(xPos + 20, yPos - 10), 
                        null, 
                        _players[i].Color, 
                        _players[i].Angle, 
                        cannonOrigin, 
                        _playerScaling, 
                        SpriteEffects.None, 
                        1
                        );
                }
            }
        }

        private void DrawText()
		{
            var player = _players[_currentPlayer];
            int currentAngle = (int)MathHelper.ToDegrees(player.Angle);

            _spriteBatch.DrawString(_font, "Cannon Angle: " + currentAngle.ToString(), new Vector2(20, 20), player.Color);
            _spriteBatch.DrawString(_font, "Cannon Power: " + player.Power.ToString(), new Vector2(20, 45), player.Color);
		}

        private void SetUpPlayers()
        {
            _players = new PlayerData[_numPlayers];
            for (int i = 0; i < _numPlayers; i++)
            {
                _players[i] = new PlayerData()
                {
                    IsAlive = true,
                    Color = _playerColors[i],
                    Angle = MathHelper.ToRadians(90),
                    Power = 100
                };
            }

            _players[0].Position = new Vector2(100, 193);
            _players[1].Position = new Vector2(200, 212);
            _players[2].Position = new Vector2(300, 361);
            _players[3].Position = new Vector2(400, 164);
        }
        
        private void ProcessKeyboard()
		{
            var keybState = Keyboard.GetState();

			if (keybState.IsKeyDown(Keys.Left))
			{
                _players[_currentPlayer].Angle -= 0.1f;
			}
			if (keybState.IsKeyDown(Keys.Right))
			{
                _players[_currentPlayer].Angle += 0.1f;
			}

            //Pi = 3.14 radians which correspond to 180 degrees, so PiOver2 radians corresponds to 90 degrees

            if (_players[_currentPlayer].Angle > MathHelper.PiOver2)
			{
                _players[_currentPlayer].Angle = MathHelper.ToRadians(90);
			}
            if(_players[_currentPlayer].Angle < -MathHelper.PiOver2)
			{
                _players[_currentPlayer].Angle = MathHelper.ToRadians(-90);
			}

			if (keybState.IsKeyDown(Keys.Down))
			{
                _players[_currentPlayer].Power -= 1;
			}
			if (keybState.IsKeyDown(Keys.Up))
			{
                _players[_currentPlayer].Power += 1;
			}

            if(_players[_currentPlayer].Power > 1000)
			{
                _players[_currentPlayer].Power = 1000; 
			}
            if(_players[_currentPlayer].Power < 0)
			{
                _players[_currentPlayer].Power = 0;
			}
		}
    }
}
