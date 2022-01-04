using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

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

    public struct ParticleData
	{
        public float BirthTime;
        public float MaxAge;
        public Vector2 OriginalPosition;
        public Vector2 Acceleration;
        public Vector2 Direction;
        public Vector2 Position;
        public float Scaling;
        public Color ModColor;
	}

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _device;
        private SpriteBatch _spriteBatch;

        private Texture2D _groundTexture;
        private Texture2D _backgroundTexture;
        private Texture2D _foregroundTexture;
        private Texture2D _carriageTexture;
        private Texture2D _cannonTexture;
        private Texture2D _explosionTexture;

        List<ParticleData> _particleList = new List<ParticleData> ();

        private int[] _terrainContour;

        private Texture2D _smokeTexture;
        private List<Vector2> _smokeList = new List<Vector2>();
        private Random _random = new Random();

        private int _numPlayers = 4;
        private PlayerData[] _players;
        private float _playerScaling;
        private int _currentPlayer = 0;
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

        private SpriteFont _font;

        private Texture2D _rocketTexture;
        private bool _rocketFlying = false;
        private Vector2 _rocketPosition;
        private Vector2 _rocketDirection;
        private float _rocketAngle;
        private float _rocketScaling = 0.1f;

        private int _screenWidth;
        private int _screenHeight;

        private Color[,] _rocketColorArray;
        private Color[,] _cannonColorArray;
        private Color[,] _carriageColorArray;
        private Color[,] _foregroundColorArray;

        private Color[,] _explosionColorArray;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        private void AddCrater(Color[,] tex, Matrix mat)
		{
            int width = tex.GetLength(0);
            int height = tex.GetLength(1);

            for(int x = 0; x < width; x++)
			{
                for(int y = 0; y < height; y++)
				{
                    if(tex[x,y].R > 10)
					{
                        var imagePos = new Vector2(x, y); // Get the pixel
                        var screenPos = Vector2.Transform(imagePos, mat); // Get the screen coordinate

                        int screenX = (int)screenPos.X;
                        int screenY = (int)screenPos.Y;

                        if(screenX > 0 && screenX < _screenWidth)
						{
                            if(_terrainContour[screenX] < screenY)
							{
                                _terrainContour[screenX] = screenY;
							}
						}


					}
				}
			}
		}

        private void AddExplosion(Vector2 explosionPos, int numOfParticles, float size, float maxAge, GameTime gameTime)
		{
            for(int i = 0; i < numOfParticles; i++)
			{
                AddExplsionParticle(explosionPos, size, maxAge, gameTime);
			}

            var rotation = (float)_random.Next(10);

            var mat = Matrix.CreateTranslation(-_explosionTexture.Width / 2, -_explosionTexture.Height / 2, 0) * Matrix.CreateRotationZ(rotation) * Matrix.CreateScale(size / (float)_explosionTexture.Width * 2.0f) * Matrix.CreateTranslation(explosionPos.X, explosionPos.Y, 0);

            AddCrater(_explosionColorArray, mat);

            for(int i = 0; i < _players.Length; i++)
			{
                _players[i].Position.Y = _terrainContour[(int)_players[i].Position.X];
			}
            FlattenTerrainBelowPlayers();
            CreateForeground();
		}

		private void AddExplsionParticle(Vector2 explosionPos, float explosionSize, float maxAge, GameTime gameTime)
		{
            var particle = new ParticleData()
            {
                OriginalPosition = explosionPos,
                Position = explosionPos,
                BirthTime = (float)gameTime.TotalGameTime.TotalMilliseconds,
                MaxAge = maxAge,
                Scaling = 0.25f,
                ModColor = Color.White
            };
            //First, we will start by determining how far the current particle should have traveled at the end of its life, we want a random value between 0 and the maximum size of the explosion.
            var particleDistance = (float)_random.NextDouble() * explosionSize;

            //We will use this particleDistance variable as the length of our direction, the only thing we need to do is rotate this over a random angle. This is done with the next three lines, first, we define a Down vector of the length found in the first line.Next, we generate a random angle, and use this random angle to create a rotation matrix, as explained in the "Angle to Direction" chapter, which is used to rotate the Down vector over the random angle.
            var displacement = new Vector2(particleDistance, 0);

            var angle = MathHelper.ToRadians(_random.Next(300));
            displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(angle));

            //particle.Direction = displacement;
            //particle.Acceleration = 3.0f * particle.Direction;

            particle.Direction = displacement * 2.0f;
            particle.Acceleration = -particle.Direction;

            _particleList.Add(particle);
		}

        private void DrawExplosion()
		{
            for(int i = 0; i < _particleList.Count; i++)
			{
                var particle = _particleList[i];
                _spriteBatch.Draw(_explosionTexture, particle.Position, null, particle.ModColor, i, new Vector2(256, 256), particle.Scaling, SpriteEffects.None, 1);
			}
		}

        private void UpdateParticles(GameTime gameTime)
		{
            var now = (float)gameTime.TotalGameTime.TotalMilliseconds;
            for(int i = _particleList.Count - 1; i >= 0 ; i--)
			{
                var particle = _particleList[i];
                var timeAlive = now - particle.BirthTime;

                if(timeAlive > particle.MaxAge)
				{
                    _particleList.RemoveAt(i);

				}
				else
				{
                    //  However, when working with time, we usually want to scale the time to a value between 0 and 1, where 0 means ‘begin’ and 1 means ‘end’, which will make a lot of the calculations easier.
                    var relAge = timeAlive / particle.MaxAge;

                    particle.Position = 0.5f * particle.Acceleration * relAge * relAge + particle.Direction * relAge + particle.OriginalPosition;
                    var invAge = 1.0f - relAge;

                    particle.ModColor = new Color(new Vector4(invAge, invAge, invAge, invAge));

                    var posFromCenter = particle.Position - particle.OriginalPosition;
                    var distance = posFromCenter.Length();

                    particle.Scaling = (50.0f + distance) / 200.0f;

                    _particleList[i] = particle;
                }
			}
		}

		private void NextPlayer()
        {
            //  if numberOfPlayers = 4, when 3 is incremented to 4 this will be reset to 0.
            _currentPlayer = _currentPlayer + 1;
            _currentPlayer = _currentPlayer % _numPlayers;


            while (!_players[_currentPlayer].IsAlive)
            {
                _currentPlayer = ++_currentPlayer % _numPlayers;
            }
        }

        private void CheckCollisions(GameTime gameTime)
        {
            var playerCollisionCoord = CheckPlayerCollision();
            if (playerCollisionCoord.X > -1)
            {
                _rocketFlying = false;
                _smokeList = new List<Vector2>();
                AddExplosion(playerCollisionCoord, 10, 80.0f, 2000.0f, gameTime);
                NextPlayer();
            }

            var terrainCollisionCoord = CheckTerrainCollision();
            if (terrainCollisionCoord.X > -1)
            {
                _rocketFlying = false;
                _smokeList = new List<Vector2>();
                AddExplosion(terrainCollisionCoord, 4, 30.0f, 1000.0f, gameTime);
                NextPlayer();
            }

            if (CheckOutOfScreen())
            {
                _rocketFlying = false;
                _smokeList = new List<Vector2>();
                NextPlayer();
            }
        }

        private bool CheckOutOfScreen()
        {
            var rocketOutOfScreen = _rocketPosition.Y > _screenHeight;
            rocketOutOfScreen |= _rocketPosition.X < 0;
            rocketOutOfScreen |= _rocketPosition.X > _screenWidth;

            return rocketOutOfScreen;
        }

        private Vector2 CheckPlayerCollision()
        {
            var rocketMat = Matrix.CreateTranslation(-24, -240, 0) *
                Matrix.CreateRotationZ(_rocketAngle) *
                Matrix.CreateScale(_rocketScaling) *
                Matrix.CreateTranslation(_rocketPosition.X, _rocketPosition.Y, 0);

            for (int i = 0; i < _numPlayers; i++)
            {
                var player = _players[i];
                if (player.IsAlive)
                {
                    if (i != _currentPlayer)
                    {
                        int xPos = (int)player.Position.X;
                        int yPos = (int)player.Position.Y;

                        var carriageMatrix = Matrix.CreateTranslation(0, -_carriageTexture.Height, 0) *
                            Matrix.CreateScale(_playerScaling) *
                            Matrix.CreateTranslation(xPos, yPos, 0);

                        var carriageCollisionPoint = TexturesCollide(_carriageColorArray, carriageMatrix, _rocketColorArray, rocketMat);

                        if (carriageCollisionPoint.X > -1)
                        {
                            _players[i].IsAlive = false;
                            return carriageCollisionPoint;
                        }

                        var cannonMat = Matrix.CreateTranslation(-11, -50, 0) *
                            Matrix.CreateRotationZ(player.Angle) *
                            Matrix.CreateScale(_playerScaling) *
                            Matrix.CreateTranslation(xPos + 20, yPos - 10, 0);

                        var cannonCollisionPoint = TexturesCollide(_cannonColorArray, cannonMat, _rocketColorArray, rocketMat);

                        if (cannonCollisionPoint.X > -1)
                        {
                            _players[i].IsAlive = false;
                            return cannonCollisionPoint;
                        }
                    }
                }
            }

            return new Vector2(-1, -1);
        }

        private Vector2 CheckTerrainCollision()
        {
            var rocketMat = Matrix.CreateTranslation(-24, -240, 0) *
                Matrix.CreateRotationZ(_rocketAngle) *
                Matrix.CreateScale(_rocketScaling) *
                Matrix.CreateTranslation(_rocketPosition.X, _rocketPosition.Y, 0);

            var terrMat = Matrix.Identity;

            var terrianCollisionPoint = TexturesCollide(_rocketColorArray, rocketMat, _foregroundColorArray, terrMat);
            return terrianCollisionPoint;
        }

        private Vector2 TexturesCollide(Color[,] tex1, Matrix mat1, Color[,] tex2, Matrix mat2)
        {
            // Using matrices, we need to transform a coordinate by mat1 and then by the inverse in mat2.

            // We can obtain the matrix that is the combination of both by simply multiplying them. 
            // Since the mat1to2 matrix is the combination of the mat1 and the inverse(mat2) matrices,
            // transforming a coordinate from Image 1 by this matrix will immediately give us the coordinate in Image 2!
            var mat1to2 = mat1 * Matrix.Invert(mat2);

            var width1 = tex1.GetLength(0);
            var height1 = tex1.GetLength(1);

            var width2 = tex2.GetLength(0);
            var height2 = tex2.GetLength(1);

            for (int x1 = 0; x1 < width1; x1++)
            {
                for (int y1 = 0; y1 < height1; y1++)
                {
                    // get the current pixel
                    var pos1 = new Vector2(x1, y1);

                    // get the screen coordinate of this pixel from 
                    var pos2 = Vector2.Transform(pos1, mat1to2);


                    // At this moment we now have 2 positions from the original images which we know if they are drawn to the same screen pixel.
                    // All we need to do now is check whether they are both non-transparen

                    int x2 = (int)pos2.X;
                    int y2 = (int)pos2.Y;

                    if ((x2 >= 0) && (x2 < width2))
                    {
                        if ((y2 >= 0) && (y2 < height2))
                        {
                            if (tex1[x1, y1].A > 0)
                            {
                                if (tex2[x2, y2].A > 0)
                                {
                                    return Vector2.Transform(pos1, mat1);
                                }
                            }
                        }
                    }

                }
            }

            return new Vector2(-1, -1);
        }

        private void GenerateTerrainContour()
        {
            _terrainContour = new int[_screenWidth];

            // The offset is the easiest one, as it simply sets the position of the midheight of the wave.
            float offset = _screenHeight / 2;
            // The peakheight value is multiplied by the output of the Sine function, so it defines how high the wave will be.
            float peakheight = 100;
            // Finally, the flatness value has an impact on the influence of X, which slows down or speeds up the wave. This increases or decreases the wavelength of our wave.
            float flatness = 70;

            double rand1 = _random.NextDouble() + 1;
            double rand2 = _random.NextDouble() + 2;
            double rand3 = _random.NextDouble() + 3;

            for (int x = 0; x < _screenWidth; x++)
            {
                //double height = peakheight * Math.Sin((float)x / flatness) + offset;
                //_terrainContour[x] = (int)height;

                // We first draw a random value between 0 and 1 together with an offset making the first wave between the [0,1] range
                var height = peakheight / rand1 * Math.Sin((float)x / flatness * rand1 + rand1);

                // We add a second wave between the [1,2] range
                height += peakheight / rand2 * Math.Sin((float)x / flatness * rand2 + rand2);

                // And finally, the last wave between the[2, 3] range.
                height += peakheight / rand3 * Math.Sin((float)x / flatness * rand3 + rand3);
                height += offset;

                _terrainContour[x] = (int)height;
            }
        }

        private void CreateForeground()
        {
            Color[,] groundColors = TextureTo2DArray(_groundTexture);

            var foregroundColors = new Color[_screenWidth * _screenHeight];

            for (int x = 0; x < _screenWidth; x++)
            {
                for (int y = 0; y < _screenHeight; y++)
                {
                    // Check if below horizontal line
                    if (y > _terrainContour[x])
                    {
                        // foregroundColors[x + y * _screenWidth] = Color.Green;
                        //foregroundColors[x + y * _screenWidth] = groundColors[x , y];
                        foregroundColors[x + y * _screenWidth] = groundColors[x % _groundTexture.Width, y % _groundTexture.Height];
                    }
                    else
                    {
                        foregroundColors[x + y * _screenWidth] = Color.Transparent;
                    }
                }
            }

            _foregroundTexture = new Texture2D(_device, _screenWidth, _screenHeight, false, SurfaceFormat.Color);
            _foregroundTexture.SetData(foregroundColors);
        }

        private void FlattenTerrainBelowPlayers()
        {
            // For each active player, the terrain underneath the cannon is levelled to the same height as the bottom-left point.

            foreach (var player in _players)
            {
                if (player.IsAlive)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        _terrainContour[(int)player.Position.X + i] = _terrainContour[(int)player.Position.X];
                    }
                }
            }
        }

        private Color[,] TextureTo2DArray(Texture2D texture)
        {
            var colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);

            Color[,] colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    colors2D[x, y] = colors1D[x + y * texture.Width];
                }
            }

            return colors2D;
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
           // _foregroundTexture = Content.Load<Texture2D>("foreground");

            _carriageTexture = Content.Load<Texture2D>("carriage");
            _cannonTexture = Content.Load<Texture2D>("cannon");

            _rocketTexture = Content.Load<Texture2D>("rocket");
            _smokeTexture = Content.Load<Texture2D>("smoke");

            _groundTexture = Content.Load<Texture2D>("ground");

            _font = Content.Load<SpriteFont>("myFont");

            _rocketColorArray = TextureTo2DArray(_rocketTexture);
            _cannonColorArray = TextureTo2DArray(_cannonTexture);
            _carriageColorArray = TextureTo2DArray(_carriageTexture);

            _explosionTexture = Content.Load<Texture2D>("explosion");

            GenerateTerrainContour();
            SetUpPlayers();
            FlattenTerrainBelowPlayers();
            CreateForeground();
            _foregroundColorArray = TextureTo2DArray(_foregroundTexture);
            _explosionColorArray = TextureTo2DArray(_explosionTexture);
            // Since the width of each flat area on the terrain is 40 pixels, this scaling factor should scale the carriage so it fits on the flat area.
            _playerScaling = 40.0f / (float)_carriageTexture.Width;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            if (_rocketFlying)
            {
                UpdateRocket();
                CheckCollisions(gameTime);
            }

            if(_particleList.Count > 0)
			{
                UpdateParticles(gameTime);
			}

			if (!_rocketFlying && _particleList.Count == 0)
			{
                ProcessKeyboard();
			}

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            DrawScenery();
            DrawPlayers();
            DrawText();
            DrawRocket();
            DrawSmoke();
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            DrawExplosion();
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
            for (int i = 0; i < _players.Length; i++)
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
                        new Vector2(0, _carriageTexture.Height), // Makes the origion the bottm-left corner
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

        private void DrawRocket()
        {
            if (_rocketFlying)
            {
                _spriteBatch.Draw(
                    _rocketTexture,
                    _rocketPosition,
                    null,
                    _players[_currentPlayer].Color,
                    _rocketAngle,
                    new Vector2(42, 240),
                    _rocketScaling,
                    SpriteEffects.None,
                    1
                    );
            }
        }

        private void DrawSmoke()
        {
            for (int i = 0; i < _smokeList.Count; i++)
            {
                _spriteBatch.Draw(_smokeTexture, _smokeList[i], null, Color.White, 0, new Vector2(40, 35), 0.2f, SpriteEffects.None, 1);
            }
        }

        private void SetUpPlayers()
        {
            _players = new PlayerData[_numPlayers];
            for (int i = 0; i < _numPlayers; i++)
            {
                var xPos = _screenWidth / (_numPlayers + 1) * (i + 1);
                _players[i] = new PlayerData()
                {
                    IsAlive = true,
                    Color = _playerColors[i],
                    Angle = MathHelper.ToRadians(90),
                    Power = 100,
                    Position = new Vector2(xPos, _terrainContour[xPos]),

                };


            }

            //_players[0].Position = new Vector2(100, 193);
            //_players[1].Position = new Vector2(200, 212);
            //_players[2].Position = new Vector2(300, 361);
            //_players[3].Position = new Vector2(400, 164);
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
            if (_players[_currentPlayer].Angle < -MathHelper.PiOver2)
            {
                _players[_currentPlayer].Angle = MathHelper.ToRadians(-90);
            }

            if (keybState.IsKeyDown(Keys.Down))
            {
                _players[_currentPlayer].Power -= 25;
            }
            if (keybState.IsKeyDown(Keys.Up))
            {
                _players[_currentPlayer].Power += 25;
            }

            if (_players[_currentPlayer].Power > 1000)
            {
                _players[_currentPlayer].Power = 1000;
            }
            if (_players[_currentPlayer].Power < 0)
            {
                _players[_currentPlayer].Power = 0;
            }

            if (keybState.IsKeyDown(Keys.Enter) || keybState.IsKeyDown(Keys.Space))
            {
                _rocketFlying = true;
                _rocketPosition = _players[_currentPlayer].Position;
                _rocketPosition.X += 20;
                _rocketPosition.Y -= 10;
                _rocketAngle = _players[_currentPlayer].Angle;

                // https://github.com/simondarksidej/XNAGameStudio/wiki/Riemers2DXNA08angletodirection#firing-the-rocket
                // A direction should have an X and Y component, indicating how many pixels horizontally and vertically the rocket should be moved each time.
                var up = new Vector2(0, -1);

                // A matrix is something you can multiply with a vector to obtain a transformed version of that vector. This creates a matrix containing a rotation around the Z axis. 
                // MonoGame can give us a matrix containing our rotation, we can use this matrix to obtain the rotated version of our vector for the rocket.
                var rotMatrix = Matrix.CreateRotationZ(_rocketAngle);

                // This will take the original up direction, transform it with the rotation around the Z axis. 
                _rocketDirection = Vector2.Transform(up, rotMatrix);

                _rocketDirection *= _players[_currentPlayer].Power / 50.0f;
            }
        }

        private void UpdateRocket()
        {
            if (_rocketFlying)
            {
                // Add the effect of gravity to the direction
                var gravity = new Vector2(0, 1);
                _rocketDirection += gravity / 10.0f;

                // Add the direction to the position
                _rocketPosition += _rocketDirection;

                // https://github.com/simondarksidej/XNAGameStudio/wiki/Riemers2DXNA09directiontoangle#rotating-rocket-along-the-direction
                // Change the angle based on the direction
                // When gravity causes the Y axis to point down, this code reflcts that.
                _rocketAngle = (float)Math.Atan2(_rocketDirection.X, -_rocketDirection.Y);

                for (int i = 0; i < 5; i++)
                {
                    var smokePos = _rocketPosition;
                    smokePos.X += _random.Next(10) - 5;
                    smokePos.Y += _random.Next(10) - 5;
                    _smokeList.Add(smokePos);
                }
            }
        }
    }
}
