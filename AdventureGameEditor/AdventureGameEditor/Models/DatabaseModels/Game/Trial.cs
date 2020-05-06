using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using AdventureGameEditor.Models.Enums;

namespace AdventureGameEditor.Models
{
    public class Trial
    {
        public int ID { get; set; }

        // What can be happens during this trial. (The options.)
        [Display(Name = "Alternatívák")]
        public virtual IList<Alternative> Alternatives { get; set; }

        // Stores if it's a luck trial or multiple choice.

        [Display(Name = "Próba típusa")]
        public TrialType TrialType { get; set; }

        public String Text { get; set; }

        
    }
}
