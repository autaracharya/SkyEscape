using SkyEscape.Scripts;
using System.IO;
using System;

public class GameSaveManager
{
    private string SaveFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SkyEscape"); // Save folder in My Documents
    private string SaveFilePath;

    public GameSaveManager()
    {
        // Ensure the directory exists
        if (!Directory.Exists(SaveFolderPath)) // Check if the save folder path exists
        {
            Directory.CreateDirectory(SaveFolderPath); // Create the folder if it doesn't exist
        }

        // Set the full file path for saving the game data
        SaveFilePath = Path.Combine(SaveFolderPath, "game_save.txt"); // Combine folder path with the file name
    }


    // Save game data (score, level, player position, and name)
    public void SaveGame(GameStateData gameStateData)
    {
        using (var writer = new StreamWriter(SaveFilePath)) // Open the file for writing
        {
            writer.WriteLine(gameStateData.Name);   // Save the player's name to the file
            writer.WriteLine(gameStateData.Score);  // Save the player's current score
            writer.WriteLine(gameStateData.Level);  // Save the current difficulty level
            writer.WriteLine(gameStateData.PlayerX); // Save the player's X position
            writer.WriteLine(gameStateData.PlayerY); // Save the player's Y position
        }
    }


    // Load game data (score, level, player position, and name)
    public GameStateData LoadGame()
    {
        if (File.Exists(SaveFilePath)) // Check if the save file exists
        {
            var lines = File.ReadAllLines(SaveFilePath); // Read all lines from the file

            // Parse the saved data from the lines
            string name = lines[0]; // Read the player's name from the first line
            int score = int.Parse(lines[1]); // Parse the player's score from the second line
            int level = int.Parse(lines[2]); // Parse the difficulty level from the third line
            float playerX = float.Parse(lines[3]); // Parse the player's X position from the fourth line
            float playerY = float.Parse(lines[4]); // Parse the player's Y position from the fifth line

            // Return a new GameStateData object initialized with the loaded data
            return new GameStateData(name, score, level, playerX, playerY);
        }

        return null; // If the save file doesn't exist, return null
    }

}
