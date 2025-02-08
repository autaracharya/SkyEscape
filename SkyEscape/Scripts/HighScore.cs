using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEscape.Scripts
{
    public class HighScore
    {
        public string Name { get; set; }
        public int Score { get; set; }

        public HighScore(string name, int score)
        {
            Name = name; // Assign the provided name to the Name property
            Score = score; // Assign the provided score to the Score property
        }

    }

}
