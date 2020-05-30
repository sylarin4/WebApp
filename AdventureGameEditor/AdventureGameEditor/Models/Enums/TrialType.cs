using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models.Enums
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
