using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models.DatabaseModels.Game
{
    // Stores an alternative what can be happen when doing a trial.
    public class Alternative
    {
        public int ID { get; set; }
        // Describes the in game story of the alternative. (For example if you try to defeat the dragon, choose this one.)
        [Display(Name = "Alternatíva leírása")]
        public String Text { get; set; }

        // Stores what happens in the game when you choose/get this alternative happen.
        [Display(Name = "Alternatíva eredménye")]
        public TrialResult TrialResult { get; set; }
    }
}
