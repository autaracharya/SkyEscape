using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SkyEscape
{
    // Enum to manage different game states
    public enum GameState
    {
        Start,
        Play,
        Help,
        Pause,
        About,
        GameOver,
        EnterName // Added for the name entry popup
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Game objects
        private Texture2D _birdTexture;// Bird Texture
        private Vector2 _birdPosition;// Bird position
        private Texture2D _pipeTexture;// Pipe texture
        private List<Vector2> _pipes;// List to store pipe positions
        private Texture2D _backgroundTexture;// Background texture
        private SpriteFont _font;// Font for drawing text
        private Texture2D _startBackgroundTexture;// Texture for start screen background
        private Texture2D _whiteTexture; // Texture for semi-transparent overlays

        // Audio variables
        private Song _startScreenMusic; // Music for the start screen
        private Song _backgroundMusic;// Music during gameplay
        private SoundEffect _flapSound;// Sound for bird flap
        private SoundEffect _gameOverSound;// Sound for game over
        private SoundEffect _hitSound;// Sound for collisions
        private SoundEffect _pointSound;// Sound for scoring points

        // Game mechanics parameters
        private const float GRAVITY = 0.5f;// Gravity affecting the bird
        private const float BIRD_FLAP_STRENGTH = -10f;// Upward force when flapping
        private const float PIPE_SPEED = 3f; // Speed of pipes moving left
        private const float PIPE_SPAWN_INTERVAL = 2f;// Time interval for spawning pipes
        private const int PIPE_WIDTH = 50; // Width of pipes
        private const int PIPE_GAP = 150;  // Space between top and bottom pipes

        private float _birdVelocity = 0;// Vertical velocity of the bird
        private float _timeSinceLastPipe = 0;// Timer to track pipe spawning
        private int _score = 0;// Player's score
        private bool _gameOver = false;// Game over flag
        private Random _random;// Random object for pipe positions

        // Difficulty progression
        private int _difficultyLevel = 1;// Difficulty level increases with score
        private float _pipeSpawnTimer = PIPE_SPAWN_INTERVAL; // Timer for spawning pipes based on difficulty

        // Scene Management
        private GameState currentState;// Current game state

        // High Score variables
        private List<HighScore> _highScores;// Current game state
        private const int MaxHighScores = 5;// Maximum number of high scores to store
        private string _playerName;// Player's name for high scores

        // To track whether the game is paused
        private bool _isPaused = false;

        private int _countdown = 3;  // Start from 3
        private float _countdownTimer = 1f;  // Timer for each countdown step
        private bool _isCountdownActive = false;  // Flag to track countdown status

        private bool _isKeyPressed = false; // To prevent repeated key presses

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _pipes = new List<Vector2>();
            _random = new Random();

            _highScores = LoadHighScores();

            // Set up game window
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();
            ResetBirdPosition();
            currentState = GameState.Start; // Start with the Start Scene
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // Create a 1x1 white texture
            _whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            _whiteTexture.SetData(new[] { Color.White });

            // Load game textures
            _birdTexture = Content.Load<Texture2D>("Images/bird");
            _pipeTexture = Content.Load<Texture2D>("Images/pipe");
            _backgroundTexture = Content.Load<Texture2D>("Images/background");
            _startBackgroundTexture = Content.Load<Texture2D>("Images/backgroundImagess");
            _font = Content.Load<SpriteFont>("default");

            // Load audio content
            _startScreenMusic = Content.Load<Song>("Music/startScreenMusic");
            _backgroundMusic = Content.Load<Song>("Music/bgMusic");
            _flapSound = Content.Load<SoundEffect>("Sounds/Flap");
            _gameOverSound = Content.Load<SoundEffect>("Sounds/GameOver");
            _hitSound = Content.Load<SoundEffect>("Sounds/Hit");
            _pointSound = Content.Load<SoundEffect>("Sounds/Point");
        }

        protected override void Update(GameTime gameTime)
        {
            // Check the current state of the game and update accordingly
            switch (currentState)
            {
                case GameState.Start:
                    // Update logic for the Start screen
                    UpdateStartScene();
                    break;

                case GameState.Play:
                    // If a countdown is active, update the countdown
                    if (_isCountdownActive)
                    {
                        UpdateCountdown(gameTime);
                    }
                    else
                    {
                        // Otherwise, update the Play scene (main gameplay)
                        UpdatePlayScene(gameTime);
                    }
                    break;

                case GameState.Help:
                    // Update logic for the Help screen
                    UpdateHelpScene();
                    break;

                case GameState.About:
                    // Update logic for the About screen
                    UpdateAboutScene();
                    break;

                case GameState.Pause:
                    // Update logic for the Pause screen
                    UpdatePauseScene();
                    break;

                case GameState.GameOver:
                    // Handle the Game Over state
                    HandleGameOver();
                    break;

                case GameState.EnterName:
                    // Update logic for the name entry screen
                    UpdateEnterNameScene();
                    break;
            }

            // Call the base Update method to ensure proper framework updates
            base.Update(gameTime);
        }


        private void UpdateStartScene()
        {
            // Get the current state of the keyboard
            var keyboardState = Keyboard.GetState();

            // Play the start screen music if it's not already playing
            if (MediaPlayer.State != MediaState.Playing)
            {
                MediaPlayer.Play(_startScreenMusic); // Start playing the start screen music
                MediaPlayer.IsRepeating = true;     // Set the music to loop
            }

            // If the Enter key is pressed, transition to the name entry screen for a new game
            if (keyboardState.IsKeyDown(Keys.Enter))
            {
                currentState = GameState.EnterName; // Switch to EnterName state to start a new game
            }

            // If the 'C' key is pressed and saved game data exists, load the saved game
            if (keyboardState.IsKeyDown(Keys.C) && SavedGameExists())
            {
                LoadGameProgress(); // Load the saved game progress and transition to play
            }

            // If the 'H' key is pressed, navigate to the Help screen
            if (keyboardState.IsKeyDown(Keys.H))
            {
                currentState = GameState.Help; // Switch to the Help state
            }

            // If the 'A' key is pressed, navigate to the About screen
            if (keyboardState.IsKeyDown(Keys.A))
            {
                currentState = GameState.About; // Switch to the About state
            }

            // If the 'Q' key is pressed, quit the game
            if (keyboardState.IsKeyDown(Keys.Q))
            {
                Exit(); // Exit the game
            }

            // If the 'D' key is pressed, delete saved game data and start fresh
            if (keyboardState.IsKeyDown(Keys.D))
            {
                DeleteSavedGame();              // Delete the saved game file
                currentState = GameState.EnterName; // Transition to the EnterName screen
            }
        }





        private void LoadGameProgress()
        {
            // Create an instance of the GameSaveManager to handle loading saved game data
            GameSaveManager saveManager = new GameSaveManager();

            // Load the saved game state from a file
            GameStateData gameStateData = saveManager.LoadGame();

            // Check if the loaded game state data is not null (i.e., a saved game exists)
            if (gameStateData != null)
            {
                // Load saved game data into the current game variables
                _playerName = gameStateData.Name;                // Set the player's name
                _score = gameStateData.Score;                   // Restore the player's score
                _difficultyLevel = gameStateData.Level;         // Restore the game's difficulty level
                _birdPosition = new Vector2(gameStateData.PlayerX, gameStateData.PlayerY); // Restore the bird's position

                // Activate a countdown before resuming gameplay
                _isCountdownActive = true; // Enable countdown mode
                _countdown = 3;            // Start the countdown from 3 seconds

                // Transition the game state to "Play" to resume gameplay
                currentState = GameState.Play;
            }
        }


        private bool SavedGameExists()
        {
            // Construct the full file path for the saved game file
            string saveFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), // Base directory (My Documents folder)
                "SkyEscape",                                                     // Subdirectory for the game
                "game_save.txt");                                                // Saved game file name

            // Check if the saved game file exists at the specified location
            return File.Exists(saveFilePath);
        }



        private void UpdateEnterNameScene()
        {
            // Get the current state of the keyboard
            var keyboardState = Keyboard.GetState();

            // Initialize _playerName if it is null or empty (to prevent crashes or invalid state)
            if (string.IsNullOrEmpty(_playerName))
            {
                _playerName = ""; // Set to an empty string if not already initialized
            }

            // Handle backspace input to remove the last character from the player's name
            if (keyboardState.IsKeyDown(Keys.Back) && _playerName.Length > 0)
            {
                _playerName = _playerName.Substring(0, _playerName.Length - 1); // Remove the last character
            }

            // Add letters to the player's name
            // Loop through all currently pressed keys
            foreach (var key in keyboardState.GetPressedKeys())
            {
                // Check if the key is a valid letter (A-Z), the name length is within the limit (20 characters),
                // and the key isn't already being held down (_isKeyPressed ensures single press behavior)
                if (key >= Keys.A && key <= Keys.Z && _playerName.Length < 20 && !_isKeyPressed)
                {
                    _playerName += key.ToString(); // Add the pressed key to the player's name
                    _isKeyPressed = true; // Set the flag to prevent repeated input
                }
            }

            // Reset the key press flag when no keys are pressed
            if (keyboardState.GetPressedKeys().Length == 0)
            {
                _isKeyPressed = false; // Allow input for the next key press
            }

            // If the Enter key is pressed and the player has entered a name, start the game
            if (keyboardState.IsKeyDown(Keys.Enter) && _playerName.Length > 0)
            {
                currentState = GameState.Play; // Transition to the Play state
            }
        }


        private void DeleteSavedGame()
        {
            // Construct the full file path for the saved game file
            string saveFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), // Base directory (My Documents folder)
                "SkyEscape",                                                     // Subdirectory for the game
                "game_save.txt");                                                // Saved game file name

            // Check if the saved game file exists at the specified location
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath); // Delete the saved game file
                Console.WriteLine("Saved game data cleared."); // Log a message to indicate the file was deleted
            }
        }


        private void UpdatePlayScene(GameTime gameTime)
        {
            // Get the current state of the keyboard
            var keyboardState = Keyboard.GetState();

            // Pause the game when 'P' is pressed, if not already paused
            if (keyboardState.IsKeyDown(Keys.P) && !_isPaused)
            {
                _isPaused = true; // Set the game to a paused state
            }

            // Resume the game when 'C' is pressed, if currently paused
            if (keyboardState.IsKeyDown(Keys.C) && _isPaused)
            {
                _isPaused = false; // Resume the game
            }

            // If the game is paused, do not proceed with game logic
            if (_isPaused)
            {
                return; // Exit the method without updating further
            }

            // Start playing background music if the game is not over and music is not already playing
            if (!_gameOver && MediaPlayer.State != MediaState.Playing)
            {
                MediaPlayer.Play(_backgroundMusic); // Play background music
                MediaPlayer.IsRepeating = true;    // Set the music to loop
            }

            // Handle game over logic
            if (_gameOver)
            {
                HandleGameOver(); // Execute game over logic
                return;           // Stop further updates during game over
            }

            // Save game progress and return to the Start Scene when 'Escape' is pressed
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                SaveGameProgress();         // Save the current game state
                currentState = GameState.Start; // Transition to the Start Scene
            }

            // Update game elements
            UpdateBirdMovement(); // Update the bird's movement (e.g., gravity and flapping)
            UpdatePipes(gameTime); // Update the pipes (e.g., spawning and moving)
            CheckCollisions();     // Check for collisions between the bird and pipes or boundaries
        }


        private void SaveGameProgress()
        {
            GameStateData gameStateData = new GameStateData(_playerName, _score, _difficultyLevel, _birdPosition.X, _birdPosition.Y); // Create a new GameStateData object with current game details
            GameSaveManager saveManager = new GameSaveManager(); // Initialize GameSaveManager to handle saving
            saveManager.SaveGame(gameStateData); // Save the current game state using the save manager
        }



        private void UpdateHelpScene()
        {
            var keyboardState = Keyboard.GetState(); // Get the current state of the keyboard

            if (keyboardState.IsKeyDown(Keys.Back)) // Check if the Backspace key is pressed
            {
                currentState = GameState.Start;  // Transition back to the Start Scene
            }

            if (keyboardState.IsKeyDown(Keys.Enter)) // Check if the Enter key is pressed
            {
                currentState = GameState.Start;  // Transition back to the Start Scene
            }
        }


        private void UpdateAboutScene()
        {
            var keyboardState = Keyboard.GetState(); // Get the current state of the keyboard

            if (keyboardState.IsKeyDown(Keys.Back)) // Check if the Backspace key is pressed
            {
                currentState = GameState.Start;  // Transition back to the Start Scene
            }

            if (keyboardState.IsKeyDown(Keys.Enter)) // Check if the Enter key is pressed
            {
                currentState = GameState.Start;  // Transition back to the Start Scene
            }
        }


        private void UpdatePauseScene()
        {
            var keyboardState = Keyboard.GetState(); // Get the current state of the keyboard

            if (keyboardState.IsKeyDown(Keys.P)) // Check if the 'P' key is pressed
            {
                currentState = GameState.Play; // Transition back to the Play state to resume the game
            }
        }


        private void UpdateCountdown(GameTime gameTime)
        {
            _countdownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds; // Decrease the countdown timer based on elapsed time

            if (_countdownTimer <= 0) // Check if the timer has reached zero
            {
                _countdown--; // Decrease the countdown value
                _countdownTimer = 1f; // Reset the timer for the next step in the countdown

                if (_countdown <= 0) // Check if the countdown has completed
                {
                    _isCountdownActive = false; // End the countdown
                }
            }
        }


        private void HandleGameOver()
        {
            var keyboardState = Keyboard.GetState(); // Get the current state of the keyboard

            // Only add the score if it's greater than 0 and the player's name is not empty
            if (_score > 0 && !string.IsNullOrEmpty(_playerName))
            {
                HighScore newHighScore = new HighScore(_playerName, _score); // Create a new high score object

                // Check if the score already exists in the high score list (based on score only)
                bool scoreExists = _highScores.Any(h => h.Score == newHighScore.Score);

                if (!scoreExists)
                {
                    _highScores.Add(newHighScore); // Add the new high score to the list
                    _highScores = _highScores.OrderByDescending(h => h.Score).Take(MaxHighScores).ToList(); // Sort and limit the list to the top scores
                    SaveHighScores();  // Save the updated high scores to a file
                }
            }

            // Restart the game if Enter is pressed
            if (keyboardState.IsKeyDown(Keys.Enter))
            {
                MediaPlayer.Stop();  // Stop any playing background music
                _gameOverSound.Play();  // Play the game over sound effect
                RestartGame(); // Reset game variables for a new session
                currentState = GameState.Play; // Transition back to the Play scene
            }

            // Return to the Start Scene if Backspace is pressed
            if (keyboardState.IsKeyDown(Keys.Back))
            {
                currentState = GameState.Start;  // Set the current state to Start
                RestartGame();  // Optionally reset game variables when returning to the Start scene
            }
        }



        private void UpdateBirdMovement()
        {
            var currentKeyboardState = Keyboard.GetState(); // Get the current state of the keyboard

            if (currentKeyboardState.IsKeyDown(Keys.Space)) // Check if the Space key is pressed
            {
                _birdVelocity = BIRD_FLAP_STRENGTH; // Apply upward velocity to simulate a flap
                _flapSound.Play(); // Play the sound effect for the bird flap
            }

            _birdVelocity += GRAVITY; // Apply gravity to the bird's velocity
            _birdPosition.Y += _birdVelocity; // Update the bird's vertical position based on its velocity
        }


        private void UpdatePipes(GameTime gameTime)
        {
            _timeSinceLastPipe += (float)gameTime.ElapsedGameTime.TotalSeconds; // Increment the timer since the last pipe was spawned

            _pipeSpawnTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds; // Decrease the pipe spawn timer
            if (_pipeSpawnTimer <= 0) // Check if it's time to spawn a new pipe
            {
                SpawnPipes(); // Spawn new pipes
                _pipeSpawnTimer = PIPE_SPAWN_INTERVAL / _difficultyLevel; // Adjust the spawn timer based on the difficulty level
            }

            for (int i = _pipes.Count - 1; i >= 0; i--) // Iterate through the list of pipes in reverse
            {
                Vector2 pipePos = _pipes[i]; // Get the current position of the pipe
                pipePos.X -= PIPE_SPEED * _difficultyLevel; // Move the pipe to the left based on its speed and difficulty level
                _pipes[i] = pipePos; // Update the pipe's position

                if (pipePos.X + PIPE_WIDTH < 0) // Check if the pipe has moved off-screen
                {
                    _pipes.RemoveAt(i); // Remove the pipe from the list
                    _score++; // Increment the player's score
                    _pointSound.Play(); // Play the sound effect for earning a point

                    if (_score % 5 == 0) // Check if the score is a multiple of 5
                    {
                        _difficultyLevel++; // Increase the difficulty level
                    }
                }
            }
        }


        private void SpawnPipes()
        {
            int screenHeight = GraphicsDevice.Viewport.Height; // Get the height of the game screen
            int pipeGapStart = _random.Next(100, screenHeight - PIPE_GAP - 100); // Calculate a random starting position for the pipe gap

            // Top pipe (positioned at the top of the screen)
            _pipes.Add(new Vector2(800, pipeGapStart)); // Add a new top pipe with its Y position based on the random gap start

            // Bottom pipe (positioned below the gap)
            _pipes.Add(new Vector2(800, pipeGapStart + PIPE_GAP)); // Add a new bottom pipe with its Y position calculated from the gap
        }

        private void CheckCollisions()
        {
            // Create a rectangle representing the bird's current position and size
            Rectangle birdRect = new Rectangle((int)_birdPosition.X, (int)_birdPosition.Y,
                                               _birdTexture.Width, _birdTexture.Height);

            // Check if the bird has hit the top or bottom screen boundaries
            if (_birdPosition.Y >= GraphicsDevice.Viewport.Height || _birdPosition.Y <= 0)
            {
                _gameOver = true; // Set the game over flag
                _hitSound.Play(); // Play the hit sound effect
                return; // Exit the method
            }

            // Check for collisions with pipes
            for (int i = 0; i < _pipes.Count; i += 2) // Iterate through the list of pipes in pairs (top and bottom)
            {
                // Create a rectangle for the top pipe
                Rectangle topPipe = new Rectangle((int)_pipes[i].X, 0, PIPE_WIDTH, (int)_pipes[i + 1].Y - (int)_pipes[i].Y);

                // Create a rectangle for the bottom pipe
                Rectangle bottomPipe = new Rectangle((int)_pipes[i + 1].X, (int)_pipes[i + 1].Y,
                                                     PIPE_WIDTH, GraphicsDevice.Viewport.Height - (int)_pipes[i + 1].Y);

                // Check if the bird's rectangle intersects with either the top or bottom pipe
                if (birdRect.Intersects(topPipe) || birdRect.Intersects(bottomPipe))
                {
                    _gameOver = true; // Set the game over flag
                    _hitSound.Play(); // Play the hit sound effect
                    break; // Exit the loop as a collision has occurred
                }
            }
        }


        // Load high scores from a file
        private List<HighScore> LoadHighScores()
        {
            List<HighScore> highScores = new List<HighScore>(); // Initialize an empty list to store high scores

            try
            {
                // Construct the file path to the high scores file in the user's "My Documents" directory
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SkyEscape", "highscores.txt");

                if (File.Exists(path)) // Check if the high scores file exists
                {
                    var lines = File.ReadAllLines(path); // Read all lines from the file
                    foreach (var line in lines) // Iterate through each line
                    {
                        var parts = line.Split(','); // Split the line into parts (name and score) separated by a comma
                        highScores.Add(new HighScore(parts[0], int.Parse(parts[1]))); // Create a HighScore object and add it to the list
                    }
                }
            }
            catch (Exception)
            {
                // If there is an error (e.g., file not found, invalid format), return an empty list
            }

            return highScores; // Return the list of high scores (empty if loading failed)
        }


        // Save high scores to a file
        private void SaveHighScores()
        {
            try
            {
                // Construct the file path to save high scores in the user's "My Documents" directory
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SkyEscape", "highscores.txt");

                // Ensure the directory exists; create it if it doesn't
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                // Open the file for writing using a StreamWriter
                using (var writer = new StreamWriter(path))
                {
                    // Write each high score to the file in "Name,Score" format
                    foreach (var score in _highScores)
                    {
                        writer.WriteLine($"{score.Name},{score.Score}");
                    }
                }
            }
            catch (Exception)
            {
                // If there is an error while saving the file, handle it silently (do nothing)
            }
        }


        // Drawing all the scenes based on game state
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue); // Clear the screen with a blue color to refresh the frame

            _spriteBatch.Begin(); // Begin drawing sprites

            // If the game is paused, draw the pause screen
            if (_isPaused)
            {
                DrawPauseScene(); // Display the "PAUSED" text and instructions to resume the game
            }
            else if (_gameOver)
            {
                DrawGameOverScene(); // Display the Game Over screen
            }
            else
            {
                // Otherwise, proceed with drawing based on the current game state
                switch (currentState)
                {
                    case GameState.Start:
                        DrawStartScene(); // Render the Start Scene
                        break;

                    case GameState.Play:
                        DrawPlayScene(); // Render the Play Scene (main gameplay)
                        if (_isCountdownActive)
                        {
                            DrawCountdown(); // Overlay the countdown if it is active
                        }
                        break;

                    case GameState.Help:
                        DrawHelpScene(); // Render the Help Scene
                        break;

                    case GameState.About:
                        DrawAboutScene(); // Render the About Scene
                        break;

                    case GameState.EnterName:
                        DrawEnterNameScene(); // Render the Enter Name Scene for player name input
                        break;
                }
            }

            _spriteBatch.End(); // End the sprite batch drawing

            base.Draw(gameTime); // Call the base class's Draw method for additional processing
        }

        private void DrawStartScene()
        {
            // Draw the background image stretched to fill the entire screen
            _spriteBatch.Draw(_startBackgroundTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);

            // Draw the game title text
            _spriteBatch.DrawString(_font, "Welcome to Sky Escape Game!", new Vector2(250, 100), Color.White);

            // Display high scores
            string highScoresText = "High Scores:"; // Header for high scores
            int yOffset = 200; // Vertical offset for high scores
            foreach (var score in _highScores) // Loop through the high scores list
            {
                highScoresText += $"\n{score.Name}: {score.Score}"; // Append each score to the text
            }
            _spriteBatch.DrawString(_font, highScoresText, new Vector2(250, yOffset), Color.LightGoldenrodYellow);

            // Display menu options
            _spriteBatch.DrawString(_font, "Press Enter to Start a New Game", new Vector2(250, 350), Color.LightGoldenrodYellow);
            _spriteBatch.DrawString(_font, "Press H for Help", new Vector2(250, 400), Color.LightGreen);
            _spriteBatch.DrawString(_font, "Press A for About", new Vector2(250, 450), Color.LightSkyBlue);

            // Display "Continue" option only if saved game data exists
            if (SavedGameExists())
            {
                _spriteBatch.DrawString(_font, "Press C to Continue from Saved Game", new Vector2(250, 500), Color.LightCoral);
            }

            // Display option to quit the game
            _spriteBatch.DrawString(_font, "Press Q to Quit", new Vector2(250, 550), Color.Red);
        }



        private void DrawEnterNameScene()
        {
            if (_playerName == null)
            {
                _playerName = "";  // Ensure _playerName is never null to prevent crashes
            }

            // Draw the background image
            _spriteBatch.Draw(_startBackgroundTexture, new Vector2(0, 0), Color.White);

            // Display the prompt for entering the player's name
            _spriteBatch.DrawString(_font, "Enter your name:", new Vector2(250, 200), Color.White);

            // Display the current input for the player's name
            _spriteBatch.DrawString(_font, _playerName, new Vector2(250, 250), Color.White);

            // Display instructions to press Enter to start the game
            _spriteBatch.DrawString(_font, "Press Enter to Start the Game", new Vector2(250, 300), Color.LightGoldenrodYellow);
        }



        private void DrawPlayScene()
        {
            // Draw the background image
            _spriteBatch.Draw(_backgroundTexture, new Vector2(0, 0), Color.White);

            // Draw the bird at its current position
            _spriteBatch.Draw(_birdTexture, _birdPosition, Color.White);

            // Draw the pipes
            for (int i = 0; i < _pipes.Count; i += 2) // Iterate through the pipes in pairs (top and bottom)
            {
                // Draw the top pipe
                _spriteBatch.Draw(_pipeTexture, new Rectangle((int)_pipes[i].X, 0, PIPE_WIDTH, (int)_pipes[i + 1].Y - (int)_pipes[i].Y), Color.White);

                // Draw the bottom pipe
                _spriteBatch.Draw(_pipeTexture,
                    new Rectangle((int)_pipes[i + 1].X, (int)_pipes[i + 1].Y,
                    PIPE_WIDTH, GraphicsDevice.Viewport.Height - (int)_pipes[i + 1].Y),
                    Color.White);
            }

            // Draw the current score and difficulty level
            _spriteBatch.DrawString(_font, $"Score: {_score} (Level: {_difficultyLevel})", new Vector2(10, 10), Color.Black);

            // Display the top high score if available
            if (_highScores.Count > 0)
            {
                var topScore = _highScores[0]; // Get the top high score from the list
                _spriteBatch.DrawString(_font, $"Top Score: {topScore.Name}: {topScore.Score}", new Vector2(10, 40), Color.Gold);
            }

            // Display instructions for pausing or saving the game
            _spriteBatch.DrawString(_font, "Press P to Pause, Esc to Save and Exit", new Vector2(10, 70), Color.White);

            // Draw Game Over text if the game is over
            if (_gameOver)
            {
                _spriteBatch.DrawString(_font, "Game Over! Press Enter to Restart", new Vector2(250, 150), Color.Red);
                _spriteBatch.DrawString(_font, " Press backspace to go home ", new Vector2(150, 200), Color.Red);
            }
        }



        private void DrawHelpScene()
        {
            // Draw the background image stretched to fit the screen dimensions
            _spriteBatch.Draw(_startBackgroundTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);

            // Draw the title "How to Play" at a specific position with a golden color
            _spriteBatch.DrawString(_font, "How to Play:", new Vector2(300, 100), Color.LightGoldenrodYellow);

            // Display instructions for gameplay
            _spriteBatch.DrawString(_font, "Press Space to make the bird flap", new Vector2(250, 150), Color.LightSkyBlue); // Instruction to control the bird
            _spriteBatch.DrawString(_font, "Avoid the pipes and try to get a high score!", new Vector2(150, 200), Color.LightGreen); // Objective of the game

            // Display instructions to return to the menu
            _spriteBatch.DrawString(_font, "Press Enter to go back to the menu", new Vector2(200, 250), Color.LightCoral); // Return to the menu
            _spriteBatch.DrawString(_font, "Press Backspace to go home", new Vector2(200, 300), Color.LightPink); // Return to the start scene
        }


        private void DrawAboutScene()
        {
            // Draw the background image stretched to fit the entire screen
            _spriteBatch.Draw(_startBackgroundTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);

            // Draw the title of the game
            _spriteBatch.DrawString(_font, "Flappy Bird Game", new Vector2(300, 100), Color.LightGoldenrodYellow);

            // Display the creator's name
            _spriteBatch.DrawString(_font, "Created by Autar Acharya", new Vector2(250, 150), Color.LightGreen);

            // Display navigation instructions
            _spriteBatch.DrawString(_font, "Press Enter to go back to the menu", new Vector2(200, 200), Color.LightSkyBlue); // Navigate to the main menu
            _spriteBatch.DrawString(_font, "Press Backspace to go home", new Vector2(200, 250), Color.LightCoral); // Navigate to the start screen
        }


        private void DrawPauseScene()
        {
            // Draw the background image stretched to cover the screen
            _spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);

            // Draw a semi-transparent black overlay to dim the screen
            _spriteBatch.Draw(_whiteTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), new Color(0, 0, 0, 128));

            // Display the "PAUSED" text at the center of the screen
            _spriteBatch.DrawString(_font, "PAUSED", new Vector2(350, 250), Color.White);

            // Display instructions for resuming the game
            _spriteBatch.DrawString(_font, "Press C to Resume", new Vector2(270, 300), Color.White);
        }

        private void DrawCountdown()
        {
            // Convert the countdown value to a string for rendering
            string countdownText = _countdown.ToString();

            // Measure the size of the countdown text to center it on the screen
            Vector2 textSize = _font.MeasureString(countdownText);

            // Calculate the position to center the text horizontally and vertically
            Vector2 position = new Vector2(
                (_graphics.PreferredBackBufferWidth - textSize.X) / 2,  // Center horizontally
                (_graphics.PreferredBackBufferHeight - textSize.Y) / 2 // Center vertically
            );

            // Draw the countdown text in red at the calculated position
            _spriteBatch.DrawString(_font, countdownText, position, Color.Red);
        }

        private void DrawGameOverScene()
        {
            // Draw the background image to cover the entire screen
            _spriteBatch.Draw(_backgroundTexture, new Vector2(0, 0), Color.White);

            // Display the "Game Over" message with instructions to restart
            _spriteBatch.DrawString(_font, "Game Over! Press Enter to Restart", new Vector2(200, 300), Color.Red);

            // Display the instruction to return to the home screen
            _spriteBatch.DrawString(_font, "Press Backspace to go home", new Vector2(200, 350), Color.Red);
        }

        private void ResetBirdPosition()
        {
            // Reset the bird's position to its starting coordinates (X: 100, Y: 200)
            _birdPosition = new Vector2(100, 200);
        }


        private void RestartGame()
        {
            ResetBirdPosition(); // Reset the bird's position to its starting coordinates
            _birdVelocity = 0; // Reset the bird's velocity to zero
            _pipes.Clear(); // Clear the list of pipes to start fresh
            _score = 0; // Reset the player's score to zero
            _gameOver = false; // Set the game over flag to false to resume gameplay
            _difficultyLevel = 1; // Reset the difficulty level to its initial value
            _pipeSpawnTimer = PIPE_SPAWN_INTERVAL; // Reset the pipe spawn timer to the default interval
        }

    }

    public class HighScore
    {
        public string Name { get; set; }
        public int Score { get; set; }

        public HighScore(string name, int score)
        {
            Name = name;
            Score = score;
        }
    }
}