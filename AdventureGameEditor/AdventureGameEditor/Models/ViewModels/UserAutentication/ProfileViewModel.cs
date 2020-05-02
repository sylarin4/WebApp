using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models
{
    public class ProfileViewModel
    {
        [Display(Name = "Felhasználó név")]
        public String UserName { get; set; }

        [Display(Name = "Nick név")]
        public String NickName { get; set; }

        [Display(Name = "Email cím")]
        public String EmailAddress { get; set; }
    }
}
