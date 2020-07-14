using System;
using System.ComponentModel.DataAnnotations;

using AdventureGameEditor.Models.Enums;

namespace AdventureGameEditor.Models.DatabaseModels.Game
{
    // Stores the result of a trial. (Used in Alternative class.)
    public class TrialResult
    {
        public int ID { get; set; }
        [Display(Name = "Alternatíva eredményéne")]
        public ResultType ResultType { get; set; }

        //Story of the result. (Describes what happens in the game when this result happens.
        [Display(Name = "Alternatíva eredményének szövege")]
        public String Text { get; set; }


    }
}
