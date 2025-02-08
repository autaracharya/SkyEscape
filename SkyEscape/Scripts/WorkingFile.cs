//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Media;
//using Microsoft.Xna.Framework.Audio;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.IO;

//namespace SkyEscape
//{
//    public enum GameState
//    {
//        Start,
//        Play,
//        Help,
//        Pause,
//        About,
//        GameOver,
//        EnterName // Added for the name entry popup
//    }

//    public class Game1 : Game
//    {
//        private GraphicsDeviceManager _graphics;
//        private SpriteBatch _spriteBatch;

//        // Game objects
//        private Texture2D _birdTexture;
//        private Vector2 _birdPosition;
//        private Texture2D _pipeTexture;
//        private List<Vector2> _pipes;
//        private Texture2D _backgroundTexture;
//        private SpriteFont _font;
//        private Texture2D _startBackgroundTexture;
//        private Texture2D _whiteTexture;

//        // Audio variables
//        private Song _startScreenMusic;
//        private Song _backgroundMusic;
//        private SoundEffect _flapSound;
//        private SoundEffect _gameOverSound;
//        private SoundEffect _hitSound;
//        private SoundEffect _pointSound;

//        // Game mechanics parameters
//        private const float GRAVITY = 0.5f;
//        private const float BIRD_FLAP_STRENGTH = -10f;
//        private const float PIPE_SPEED = 3f;
//        private const float PIPE_SPAWN_INTERVAL = 2f;
//        private const int PIPE_WIDTH = 50;
//        private const int PIPE_GAP = 150;  // Space between top and bottom pipes

//        private float _birdVelocity = 0;
//        private float _timeSinceLastPipe = 0;
//        private int _score = 0;
//        private bool _gameOver = false;
//        private Random _random;

//        // Difficulty progression
//        private int _difficultyLevel = 1;
//        private float _pipeSpawnTimer = PIPE_SPAWN_INTERVAL;

//        // Scene Management
//        private GameState currentState;

//        // High Score variables
//        private List<HighScore> _highScores;
//        private const int MaxHighScores = 5;
//        private string _playerName;

//        // To track whether the game is paused
//        private bool _isPaused = false;

//        private bool _isKeyPressed = false; // To prevent repeated key presses

//        public Game1()
//        {
//            _graphics = new GraphicsDeviceManager(this);
//            Content.RootDirectory = "Content";
//            _pipes = new List<Vector2>();
//            _random = new Random();

//            _highScores = LoadHighScores();

//            // Set up game window
//            _graphics.PreferredBackBufferWidth = 800;
//            _graphics.PreferredBackBufferHeight = 600;
//            _graphics.ApplyChanges();
//        }

//        protected override void Initialize()
//        {
//            base.Initialize();
//            ResetBirdPosition();
//            currentState = GameState.Start; // Start with the Start Scene
//        }

//        protected override void LoadContent()
//        {
//            _spriteBatch = new SpriteBatch(GraphicsDevice);
//            // Create a 1x1 white texture
//            _whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
//            _whiteTexture.SetData(new[] { Color.White });

//            // Load game textures
//            _birdTexture = Content.Load<Texture2D>("Images/bird");
//            _pipeTexture = Content.Load<Texture2D>("Images/pipe");
//            _backgroundTexture = Content.Load<Texture2D>("Images/background");
//            _startBackgroundTexture = Content.Load<Texture2D>("Images/backgroundImage");
//            _font = Content.Load<SpriteFont>("default");

//            // Load audio content
//            _startScreenMusic = Content.Load<Song>("Music/startScreenMusic");
//            _backgroundMusic = Content.Load<Song>("Music/bgMusic");
//            _flapSound = Content.Load<SoundEffect>("Sounds/Flap");
//            _gameOverSound = Content.Load<SoundEffect>("Sounds/GameOver");
//            _hitSound = Content.Load<SoundEffect>("Sounds/Hit");
//            _pointSound = Content.Load<SoundEffect>("Sounds/Point");
//        }

//        protected override void Update(GameTime gameTime)
//        {
//            // Handle different scenes based on current state
//            switch (currentState)
//            {
//                case GameState.Start:
//                    UpdateStartScene();
//                    break;

//                case GameState.Play:
//                    UpdatePlayScene(gameTime);
//                    break;

//                case GameState.Help:
//                    UpdateHelpScene();
//                    break;

//                case GameState.About:
//                    UpdateAboutScene();
//                    break;

//                case GameState.Pause:
//                    UpdatePauseScene();
//                    break;

//                case GameState.GameOver:
//                    HandleGameOver();
//                    break;

//                case GameState.EnterName:
//                    UpdateEnterNameScene();
//                    break;
//            }

//            base.Update(gameTime);
//        }

//        private void UpdateStartScene()
//        {
//            var keyboardState = Keyboard.GetState();

//            // Play start screen music if it isn't already playing
//            if (MediaPlayer.State != MediaState.Playing)
//            {
//                MediaPlayer.Play(_startScreenMusic);  // Play the start screen music
//                MediaPlayer.IsRepeating = true;  // Loop the start screen music
//            }

//            // Detect when the player presses Enter to start the game or view Help/About
//            if (keyboardState.IsKeyDown(Keys.Enter))
//            {
//                //ClearHighScores();
//                currentState = GameState.EnterName;  // Go to Enter Name scene
//            }

//            if (keyboardState.IsKeyDown(Keys.H))
//            {
//                currentState = GameState.Help;  // Go to help scene
//            }

//            if (keyboardState.IsKeyDown(Keys.A))
//            {
//                currentState = GameState.About;  // Go to about scene
//            }
//        }

//        //private void ClearHighScores()
//        //{
//        //    _highScores.Clear();  // Clear the high score list for this session
//        //    SaveHighScores();     // Save the cleared list (this will write an empty list to the file)
//        //}

//        private void UpdateEnterNameScene()
//        {
//            var keyboardState = Keyboard.GetState();

//            // Initialize _playerName if it is null or empty (prevents crashes)
//            if (string.IsNullOrEmpty(_playerName))
//            {
//                _playerName = "";
//            }

//            // Handle name input
//            if (keyboardState.IsKeyDown(Keys.Back) && _playerName.Length > 0)
//            {
//                _playerName = _playerName.Substring(0, _playerName.Length - 1);  // Remove last character
//            }

//            // Add letters to the player's name (single press check)
//            foreach (var key in keyboardState.GetPressedKeys())
//            {
//                if (key >= Keys.A && key <= Keys.Z && _playerName.Length < 20 && !_isKeyPressed)
//                {
//                    _playerName += key.ToString();
//                    _isKeyPressed = true; // Set flag to true when a key is pressed
//                }
//            }

//            // Reset the key press flag when no keys are pressed
//            if (keyboardState.GetPressedKeys().Length == 0)
//            {
//                _isKeyPressed = false;
//            }

//            // Once the name is entered, start the game
//            if (keyboardState.IsKeyDown(Keys.Enter) && _playerName.Length > 0)
//            {
//                currentState = GameState.Play;  // Start the game
//            }
//        }



//        private void UpdatePlayScene(GameTime gameTime)
//        {
//            var keyboardState = Keyboard.GetState();

//            // Pause the game when 'P' is pressed
//            if (keyboardState.IsKeyDown(Keys.P) && !_isPaused)
//            {
//                _isPaused = true; // Set the game state to paused
//            }

//            // Resume the game when 'C' is pressed
//            if (keyboardState.IsKeyDown(Keys.C) && _isPaused)
//            {
//                _isPaused = false; // Set the game state to resume
//            }

//            // If the game is paused, do not process any game logic
//            if (_isPaused)
//            {
//                return; // Stop updating game logic if paused
//            }

//            // Process the game logic when not paused
//            if (!_gameOver && MediaPlayer.State != MediaState.Playing)
//            {
//                MediaPlayer.Play(_backgroundMusic); // Play background music
//                MediaPlayer.IsRepeating = true;
//            }

//            if (_gameOver)
//            {
//                HandleGameOver();
//                return;
//            }

//            UpdateBirdMovement();
//            UpdatePipes(gameTime);
//            CheckCollisions();
//        }

//        private void UpdateHelpScene()
//        {
//            var keyboardState = Keyboard.GetState();

//            // Return to start screen if Backspace is pressed
//            if (keyboardState.IsKeyDown(Keys.Back))
//            {
//                currentState = GameState.Start;  // Go back to Start Scene
//            }

//            // Press Enter to return to the main menu (Start Scene)
//            if (keyboardState.IsKeyDown(Keys.Enter))
//            {
//                currentState = GameState.Start;  // Go back to Start Scene
//            }
//        }

//        private void UpdateAboutScene()
//        {
//            var keyboardState = Keyboard.GetState();

//            // Return to start screen if Backspace is pressed
//            if (keyboardState.IsKeyDown(Keys.Back))
//            {
//                currentState = GameState.Start;  // Go back to Start Scene
//            }

//            // Press Enter to return to the main menu (Start Scene)
//            if (keyboardState.IsKeyDown(Keys.Enter))
//            {
//                currentState = GameState.Start;  // Go back to Start Scene
//            }
//        }

//        private void UpdatePauseScene()
//        {
//            var keyboardState = Keyboard.GetState();

//            // Check if the player presses 'P' to resume the game
//            if (keyboardState.IsKeyDown(Keys.P))
//            {
//                currentState = GameState.Play; // Unpause the game
//            }
//        }

//        private void HandleGameOver()
//        {
//            var keyboardState = Keyboard.GetState();

//            // Only add the score if it's greater than 0 and it's a unique score
//            if (_score > 0 && !string.IsNullOrEmpty(_playerName))
//            {
//                HighScore newHighScore = new HighScore(_playerName, _score);

//                // Check if the score already exists in the list (score only, not the player's name)
//                bool scoreExists = _highScores.Any(h => h.Score == newHighScore.Score);

//                if (!scoreExists)
//                {
//                    // Add new score to the list and sort the list
//                    _highScores.Add(newHighScore);
//                    _highScores = _highScores.OrderByDescending(h => h.Score).Take(MaxHighScores).ToList();
//                    SaveHighScores();  // Save high scores to file
//                }
//            }

//            // Restart the game if Enter is pressed
//            if (keyboardState.IsKeyDown(Keys.Enter))
//            {
//                MediaPlayer.Stop();  // Stop background music
//                _gameOverSound.Play();  // Play game over sound
//                RestartGame();
//                currentState = GameState.Play; // Transition to the Play scene
//            }

//            // Go back to Start Scene if Backspace is pressed
//            if (keyboardState.IsKeyDown(Keys.Back))
//            {
//                currentState = GameState.Start;  // Go back to Start Scene
//                RestartGame();  // Optionally reset the game state when going back to Start
//            }
//        }


//        private void UpdateBirdMovement()
//        {
//            var currentKeyboardState = Keyboard.GetState();

//            if (currentKeyboardState.IsKeyDown(Keys.Space))
//            {
//                _birdVelocity = BIRD_FLAP_STRENGTH;
//                _flapSound.Play();  // Play flap sound
//            }

//            _birdVelocity += GRAVITY;
//            _birdPosition.Y += _birdVelocity;
//        }

//        private void UpdatePipes(GameTime gameTime)
//        {
//            _timeSinceLastPipe += (float)gameTime.ElapsedGameTime.TotalSeconds;

//            _pipeSpawnTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
//            if (_pipeSpawnTimer <= 0)
//            {
//                SpawnPipes();
//                _pipeSpawnTimer = PIPE_SPAWN_INTERVAL / _difficultyLevel;
//            }

//            for (int i = _pipes.Count - 1; i >= 0; i--)
//            {
//                Vector2 pipePos = _pipes[i];
//                pipePos.X -= PIPE_SPEED * _difficultyLevel;
//                _pipes[i] = pipePos;

//                if (pipePos.X + PIPE_WIDTH < 0)
//                {
//                    _pipes.RemoveAt(i);
//                    _score++;
//                    _pointSound.Play();  // Play point sound

//                    if (_score % 5 == 0)
//                    {
//                        _difficultyLevel++;  // Increase difficulty level
//                    }
//                }
//            }
//        }

//        private void SpawnPipes()
//        {
//            int screenHeight = GraphicsDevice.Viewport.Height;
//            int pipeGapStart = _random.Next(100, screenHeight - PIPE_GAP - 100);

//            // Top pipe (positioned at the top)
//            _pipes.Add(new Vector2(800, pipeGapStart));  // Y position for the top pipe

//            // Bottom pipe (positioned below the gap)
//            _pipes.Add(new Vector2(800, pipeGapStart + PIPE_GAP));  // Y position for the bottom pipe with gap
//        }

//        private void CheckCollisions()
//        {
//            Rectangle birdRect = new Rectangle((int)_birdPosition.X, (int)_birdPosition.Y,
//                                               _birdTexture.Width, _birdTexture.Height);

//            // Check screen boundaries
//            if (_birdPosition.Y >= GraphicsDevice.Viewport.Height || _birdPosition.Y <= 0)
//            {
//                _gameOver = true;
//                _hitSound.Play();  // Play hit sound
//                return;
//            }

//            // Check pipe collisions
//            for (int i = 0; i < _pipes.Count; i += 2)
//            {
//                Rectangle topPipe = new Rectangle((int)_pipes[i].X, 0, PIPE_WIDTH, (int)_pipes[i + 1].Y - (int)_pipes[i].Y);
//                Rectangle bottomPipe = new Rectangle((int)_pipes[i + 1].X, (int)_pipes[i + 1].Y,
//                                                     PIPE_WIDTH, GraphicsDevice.Viewport.Height - (int)_pipes[i + 1].Y);

//                if (birdRect.Intersects(topPipe) || birdRect.Intersects(bottomPipe))
//                {
//                    _gameOver = true;
//                    _hitSound.Play();  // Play hit sound
//                    break;
//                }
//            }
//        }

//        // Load high scores from a file
//        private List<HighScore> LoadHighScores()
//        {
//            List<HighScore> highScores = new List<HighScore>();

//            try
//            {
//                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SkyEscape", "highscores.txt");
//                if (File.Exists(path))
//                {
//                    var lines = File.ReadAllLines(path);
//                    foreach (var line in lines)
//                    {
//                        var parts = line.Split(',');
//                        highScores.Add(new HighScore(parts[0], int.Parse(parts[1])));
//                    }
//                }
//            }
//            catch (Exception)
//            {
//                // If there is an error loading the file, return an empty list
//            }

//            return highScores;
//        }

//        // Save high scores to a file
//        private void SaveHighScores()
//        {
//            try
//            {
//                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SkyEscape", "highscores.txt");
//                Directory.CreateDirectory(Path.GetDirectoryName(path));

//                using (var writer = new StreamWriter(path))
//                {
//                    foreach (var score in _highScores)
//                    {
//                        writer.WriteLine($"{score.Name},{score.Score}");
//                    }
//                }
//            }
//            catch (Exception)
//            {
//                // If there is an error saving the file, do nothing
//            }
//        }

//        // Drawing all the scenes based on game state
//        protected override void Draw(GameTime gameTime)
//        {
//            GraphicsDevice.Clear(Color.CornflowerBlue); // Clear the screen

//            _spriteBatch.Begin();

//            // If the game is paused, draw the pause screen
//            if (_isPaused)
//            {
//                DrawPauseScene(); // This method displays the "PAUSED" text and resume instructions
//            }
//            else if (_gameOver)
//            {
//                DrawGameOverScene(); // This method displays the Game Over screen
//            }
//            else
//            {
//                // Otherwise, proceed with drawing the normal game scenes
//                switch (currentState)
//                {
//                    case GameState.Start:
//                        DrawStartScene();
//                        break;

//                    case GameState.Play:
//                        DrawPlayScene();
//                        break;

//                    case GameState.Help:
//                        DrawHelpScene();
//                        break;

//                    case GameState.About:
//                        DrawAboutScene();
//                        break;

//                    case GameState.EnterName:
//                        DrawEnterNameScene();
//                        break;
//                }
//            }

//            _spriteBatch.End();

//            base.Draw(gameTime);
//        }

//        private void DrawStartScene()
//        {
//            _spriteBatch.Draw(_startBackgroundTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
//            _spriteBatch.DrawString(_font, "Welcome to Sky Escape Game!", new Vector2(250, 100), Color.White);

//            // Display the top 5 high scores with player names
//            string highScoresText = "High Scores:";
//            int yOffset = 200;
//            foreach (var score in _highScores)
//            {
//                highScoresText += $"\n{score.Name}: {score.Score}";
//            }
//            _spriteBatch.DrawString(_font, highScoresText, new Vector2(250, yOffset), Color.LightGoldenrodYellow);

//            string menuText = "Press Enter to Start\nH for Help\nA for About";
//            Vector2 position = new Vector2(250, 350); // Position starting point
//            _spriteBatch.DrawString(_font, "Press Enter to Start", new Vector2(position.X, position.Y), Color.LightGoldenrodYellow);
//            position.Y += 40; // Space between the lines
//            _spriteBatch.DrawString(_font, "Press H for Help", new Vector2(position.X, position.Y), Color.LightGreen);
//            position.Y += 40;
//            _spriteBatch.DrawString(_font, "Press A for About", new Vector2(position.X, position.Y), Color.LightSkyBlue);
//        }


//        private void DrawEnterNameScene()
//        {
//            if (_playerName == null)
//            {
//                _playerName = "";  // Ensure _playerName is never null
//            }

//            _spriteBatch.DrawString(_font, "Enter your name:", new Vector2(250, 200), Color.White);
//            _spriteBatch.DrawString(_font, _playerName, new Vector2(250, 250), Color.White);
//            _spriteBatch.DrawString(_font, "Press Enter to Start the Game", new Vector2(250, 300), Color.LightGoldenrodYellow);
//        }


//        private void DrawPlayScene()
//        {
//            // Draw background
//            _spriteBatch.Draw(_backgroundTexture, new Vector2(0, 0), Color.White);

//            // Draw bird
//            _spriteBatch.Draw(_birdTexture, _birdPosition, Color.White);

//            // Draw pipes
//            for (int i = 0; i < _pipes.Count; i += 2)
//            {
//                // Draw top pipe
//                _spriteBatch.Draw(_pipeTexture, new Rectangle((int)_pipes[i].X, 0, PIPE_WIDTH, (int)_pipes[i + 1].Y - (int)_pipes[i].Y), Color.White);

//                // Draw bottom pipe
//                _spriteBatch.Draw(_pipeTexture,
//                    new Rectangle((int)_pipes[i + 1].X, (int)_pipes[i + 1].Y,
//                    PIPE_WIDTH, GraphicsDevice.Viewport.Height - (int)_pipes[i + 1].Y),
//                    Color.White);
//            }

//            // Draw score and level
//            _spriteBatch.DrawString(_font, $"Score: {_score} (Level: {_difficultyLevel})", new Vector2(10, 10), Color.Black);

//            // Display the top 1 high score below score and level
//            if (_highScores.Count > 0)
//            {
//                var topScore = _highScores[0]; // Top high score is the first one in the list (sorted by descending order)
//                _spriteBatch.DrawString(_font, $"Top Score: {topScore.Name}: {topScore.Score}", new Vector2(10, 40), Color.Gold);
//            }

//            // Draw Game Over text if game is over
//            if (_gameOver)
//            {
//                _spriteBatch.DrawString(_font, "Game Over! Press Enter to Restart", new Vector2(250, 150), Color.Red);
//                _spriteBatch.DrawString(_font, "Game Over! Press Enter to Restart", new Vector2(150, 200), Color.Red);
//            }
//        }


//        private void DrawHelpScene()
//        {
//            _spriteBatch.Draw(_startBackgroundTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
//            _spriteBatch.DrawString(_font, "How to Play:", new Vector2(300, 100), Color.LightGoldenrodYellow);
//            _spriteBatch.DrawString(_font, "Press Space to make the bird flap", new Vector2(250, 150), Color.LightSkyBlue);
//            _spriteBatch.DrawString(_font, "Avoid the pipes and try to get a high score!", new Vector2(150, 200), Color.LightGreen);
//            _spriteBatch.DrawString(_font, "Press Enter to go back to the menu", new Vector2(200, 250), Color.LightCoral);
//            _spriteBatch.DrawString(_font, "Press Backspace to go home", new Vector2(200, 300), Color.LightPink);
//        }

//        private void DrawAboutScene()
//        {
//            _spriteBatch.Draw(_startBackgroundTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
//            _spriteBatch.DrawString(_font, "Flappy Bird Game", new Vector2(300, 100), Color.LightGoldenrodYellow);
//            _spriteBatch.DrawString(_font, "Created by Autar Acharya", new Vector2(250, 150), Color.LightGreen);
//            _spriteBatch.DrawString(_font, "Press Enter to go back to the menu", new Vector2(200, 200), Color.LightSkyBlue);
//            _spriteBatch.DrawString(_font, "Press Backspace to go home", new Vector2(200, 250), Color.LightCoral);
//        }

//        private void DrawPauseScene()
//        {
//            _spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);
//            _spriteBatch.Draw(_whiteTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), new Color(0, 0, 0, 128)); // Semi-transparent black overlay
//            _spriteBatch.DrawString(_font, "PAUSED", new Vector2(350, 250), Color.White);
//            _spriteBatch.DrawString(_font, "Press C to Resume", new Vector2(270, 300), Color.White);
//        }

//        private void DrawGameOverScene()
//        {
//            _spriteBatch.Draw(_backgroundTexture, new Vector2(0, 0), Color.White);
//            _spriteBatch.DrawString(_font, "Game Over! Press Enter to Restart", new Vector2(200, 300), Color.Red);
//            _spriteBatch.DrawString(_font, "Press Backspace to go home", new Vector2(200, 350), Color.Red);
//        }

//        private void ResetBirdPosition()
//        {
//            _birdPosition = new Vector2(100, 200);
//        }

//        private void RestartGame()
//        {
//            ResetBirdPosition();
//            _birdVelocity = 0;
//            _pipes.Clear();
//            _score = 0;
//            _gameOver = false;
//            _difficultyLevel = 1;
//            _pipeSpawnTimer = PIPE_SPAWN_INTERVAL;
//        }
//    }

//    public class HighScore
//    {
//        public string Name { get; set; }
//        public int Score { get; set; }

//        public HighScore(string name, int score)
//        {
//            Name = name;
//            Score = score;
//        }
//    }
//}
