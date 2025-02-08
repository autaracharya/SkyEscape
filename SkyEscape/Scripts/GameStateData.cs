public class GameStateData
{
    public string Name { get; set; }
    public int Score { get; set; }
    public int Level { get; set; }
    public float PlayerX { get; set; }
    public float PlayerY { get; set; }

    public GameStateData(string name, int score, int level, float playerX, float playerY)
    {
        Name = name;      // Assign the provided name to the Name property
        Score = score;    // Assign the provided score to the Score property
        Level = level;    // Assign the provided level to the Level property
        PlayerX = playerX; // Assign the provided X-coordinate to the PlayerX property
        PlayerY = playerY; // Assign the provided Y-coordinate to the PlayerY property
    }

}
