using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    // Used by the Guests' Create view to register a new guest.
    public class RegisterGuestViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [RegularExpression("^[A-Za-z0-9_-]{5,40}$", ErrorMessage = "Name's format or length is not valid.")]
        public String Name { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public String Address { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Phone number's format isn't valid.")]
        [DataType(DataType.PhoneNumber)]
        public String PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression("^[A-Za-z0-9_-]{5,40}$", ErrorMessage = "Password's format or length is not valid.")]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        [Required(ErrorMessage = "Az e-mail cím megadása kötelező.")]
        [EmailAddress(ErrorMessage = "Az e-mail cím nem megfelelő formátumú.")]
        [DataType(DataType.EmailAddress)] 
        public String Email { get; set; }
    }
}
