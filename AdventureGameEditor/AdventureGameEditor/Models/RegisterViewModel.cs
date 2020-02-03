using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "A név mezőt kötelező kitölteni.")]
        public String Name { get; set; }

        [Required(ErrorMessage = "A játékon belüli név mezőt kötelező kitölteni.")]
        public String NickName { get; set; }

        [Required(ErrorMessage = "A jelszó mezőt kötelező kitölteni.")]
        [RegularExpression("^[A-Za-z0-9_-]{8,40}$", ErrorMessage = "A jelszó formátuma nem megfelelő.")]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        [Required(ErrorMessage = "Az email mezőt kötelező kitölteni.")]
        public String Email { get; set; }
    }
}
