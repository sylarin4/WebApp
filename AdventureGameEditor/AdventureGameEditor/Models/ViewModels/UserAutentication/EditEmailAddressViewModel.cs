using System;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models.ViewModels.UserAutentication
{ 
    public class EditEmailAddressViewModel
    {
        [Display(Name = "Új email cím")]
        [Required(ErrorMessage = "Az email cím mezőjét kötelező kitölteni.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Nem megfelelő email formátum.")]
        public String NewEmailAddress { get; set; }

        [Display(Name = "Új email cím megerősítése")]
        [Required(ErrorMessage = "Az email cím megerősítése mezőjét kötelező kitölteni.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Nem megfelelő email formátum.")]
        [Compare(nameof(NewEmailAddress), ErrorMessage = "Az email címek nem egyeznek.")]
        public String ValidateEmailAddress { get; set; }
    }
}
