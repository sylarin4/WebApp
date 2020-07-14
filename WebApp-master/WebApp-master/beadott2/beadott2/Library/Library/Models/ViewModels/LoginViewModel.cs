using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace Library.Models
{
    // Used by the Guests' Login view to login a registered guest.
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public String Name { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public String Password { get; set; }
    }
}
