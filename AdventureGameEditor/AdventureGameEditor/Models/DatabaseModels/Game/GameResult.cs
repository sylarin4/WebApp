using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models.DatabaseModels.Game
{
    // Represents the game result: the text, that appears if the player won or lose.
    public class GameResult
    {
        public int ID { get; set; }
        public User Owner { get; set; }
        public String GameTitle { get; set; }
        // If the data concern to a won game it's true, if to a lost match, it's false.
        public Boolean IsGameWonResult { get; set; }

        public Image? Image { get; set; }
        public String Text { get; set; }
    }
}
