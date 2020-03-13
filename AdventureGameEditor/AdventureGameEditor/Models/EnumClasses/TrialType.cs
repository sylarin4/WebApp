using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models
{
    // Stores the type of a trial for Trial class.
    public enum TrialType
    {
        [Display(Name = "Szerencsepróba")]
        LuckTrial,
        [Display(Name = "Többszörös választás")]
        MultipleChoice
    }
}
