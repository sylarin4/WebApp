using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    // Stores the data of a game.
    public class Game
    {
        public int ID { get; set; }

        // The game's creator.
        public User Owner { get; set; }
        public String Title { get; set; }

        // Stores who can see and play this game.
        public Visibility Visibility { get; set; }

        // Stores how many times this game was played.
        public int PlayCounter { get; set; }

        // Stores the size of the map.
        public int TableSize { get; set; }

        // Stores the map of the game.
        public virtual ICollection<MapRow> Map { get; set; }

        // Stores on which field the game starts.
        public Field StartField { get; set; }

        // Stores the player should reach which field to win the game.
        public Field TargetField { get; set; }

        // Stores that which type of map picece is being put on the map.
        public int CurrentWayDirectionsCode { get; set; }
    }
}
