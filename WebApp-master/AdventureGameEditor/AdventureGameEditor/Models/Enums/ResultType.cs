using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models.Enums
{
    // Enumerates the type of results of a trial. (Used in TrialResult class.)
    public enum ResultType
    {
        [Display(Name = "Semmi")]
        Nothing,
        [Display(Name = "Egy életpont levonása")]
        LoseLife,
        [Display(Name ="Játék megnyerése")]
        GameWon,
        [Display(Name = "Teleportáció")]
        Teleport,
        [Display(Name ="Egy életpont adása")]
        GetLife,
        [Display(Name ="Iránymutatás a célmezőhöz")]
        GetTargetDirection
    }
}
