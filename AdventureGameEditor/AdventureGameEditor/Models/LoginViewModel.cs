using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models
{
    public class LoginViewModel
    {
        [Display(Name = "Felhasználó név")]
        [Required(ErrorMessage = "A felhasználó név mezője nem lehet üres.")]
        public String UserName { get; set; }

        [Display(Name = "Jelszó")]
        [Required(ErrorMessage = "A jelszó mezője nem lehet üres.")]
        [DataType(DataType.Password)]
        public String Password { get; set; }
    }
}
