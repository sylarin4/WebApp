using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    // Stores an alternative what can be happen when doing a trial.
    public class Alternative
    {

        // Describes the in game story of the alternative. (For example if you try to defeat the dragon, choose this one.)
        public String Text { get; set; }

        // Stores what happens in the game when you choose/get this alternative happen.
        public TrialResult TrialResult { get; set; }
    }
}
