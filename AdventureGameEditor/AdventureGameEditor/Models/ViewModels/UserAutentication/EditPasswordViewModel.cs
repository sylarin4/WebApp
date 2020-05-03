using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models
{
    public class EditPasswordViewModel
    {
        [Display(Name = "Új jelszó")]
        [Required(ErrorMessage = "A jelszó mezőt kötelező kitölteni.")]
        [RegularExpression("^[A-Za-z0-9_-]{8,40}$", ErrorMessage = "A jelszó formátuma nem megfelelő.")]
        [DataType(DataType.Password)]
        public String NewPassword { get; set; }

        [Display(Name = "Új jelszó megerősítése")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "A jelszavak nem egyeznek.")]
        public String ValidatePassword { get; set; }
    }
}
