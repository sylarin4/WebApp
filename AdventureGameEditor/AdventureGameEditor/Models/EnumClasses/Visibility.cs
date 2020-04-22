using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models
{
    // Enum class for the Game class visibility attribute.
    public enum Visibility
    {
        [Display(Name = "Csak én")]
        Owner,
        [Display(Name = "Minden bejelentkezett felhasználó")]
        LoggedIn,
        [Display(Name = "Mindenki")]
        Everyone
    }
}
