using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    // Stores the result of a trial. (Used in Alternative class.)
    public class TrialResult
    {
        public ResultType ResultType { get; set; }

        // If the result is teleporttation, stores the coordinates of the field where to teleport.
        public FieldCoordinate TeleportTarget { get; set; } 

        //Story of the result. (Describes what happens in the game when this result happens.
        public String Text { get; set; }


    }
}
