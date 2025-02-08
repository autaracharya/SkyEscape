using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEscape.Scripts
{
    public class HighScoreManager
    {
        private string HighScoreFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SkyEscape");  // Save folder in My Documents
        private string HighScoreFilePath;

        private List<HighScore> highScores;

        public HighScoreManager()
        {
            highScores = new List<HighScore>(); // Initialize the list of high scores

            // Ensure the directory for high scores exists
            if (!Directory.Exists(HighScoreFolderPath)) // Check if the high score folder path exists
            {
                Directory.CreateDirectory(HighScoreFolderPath); // Create the folder if it doesn't exist
            }

            HighScoreFilePath = Path.Combine(HighScoreFolderPath, "highscores.txt"); // Combine folder path with the file name

            LoadHighScores(); // Load existing high scores from the file when the game starts
        }


        // Save high scores to the file
        public void SaveHighScores()
        {
            using (var writer = new StreamWriter(HighScoreFilePath)) // Open the high score file for writing
            {
                foreach (var score in highScores) // Iterate through the list of high scores
                {
                    writer.WriteLine($"{score.Name},{score.Score}"); // Write each high score in "Name,Score" format
                }
            }
        }


        // Load high scores from the file
        public void LoadHighScores()
        {
            if (File.Exists(HighScoreFilePath))  // If the file exists
            {
                var lines = File.ReadAllLines(HighScoreFilePath);  // Read all lines
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    highScores.Add(new HighScore(parts[0], int.Parse(parts[1])));  // Add high score to the list
                }
            }
        }

        // Add a new high score
        public void AddScore(string name, int score)
        {
            highScores.Add(new HighScore(name, score)); // Add a new high score to the list

            // Sort the list in descending order by score and keep only the top 5 scores
            highScores = highScores.OrderByDescending(h => h.Score).Take(5).ToList();
        }


        // Get the top 5 high scores
        public List<HighScore> GetTopScores()
        {
            // Sort the high scores in descending order by score and return the top 5
            return highScores.OrderByDescending(h => h.Score).Take(5).ToList();
        }

    }
}
