using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Serialization;

namespace AdventureGameEditor.Models.ViewModels.UserAutentication
{ 
    public class EditEmailAddressViewModel
    {
        [Display(Name = "Új email cím")]
        [Required(ErrorMessage = "Az email mezőt kötelező kitölteni.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Nem megfelelő email formátum.")]
        public String NewEmailAddress { get; set; }

        [Display(Name = "Új email cím megerősítése")]
        [Required(ErrorMessage = "Az email cím megerősítése mezőt kötelező kitölteni.")]
        [DataType(DataType.EmailAddress,ErrorMessage = "Nem megfelelő email formátum.")]
        [Compare(nameof(NewEmailAddress), ErrorMessage = "Az email címek nem egyeznek.")]
        public String ValidateEmailAddress { get; set; }
    }
}
