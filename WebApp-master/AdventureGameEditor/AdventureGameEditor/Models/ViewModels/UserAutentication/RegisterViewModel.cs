using System;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models.ViewModels.UserAutentication
{
    public class RegisterViewModel
    {
        [Display(Name = "Felhasználó név")]
        [Required(ErrorMessage = "A felhasználó név mezőjét kötelező kitölteni.")]
        [RegularExpression("^[A-Za-z0-9_-]{4,40}$", ErrorMessage = "A felhasználó név formátuma nem megfelelő.")]
        public String UserName { get; set; }

        [Display(Name = "Becenév (amit a többi felhasználó lát)")]
        [Required(ErrorMessage = "A becenév mezőt kötelező kitölteni.")]
        [RegularExpression("^[A-Za-z0-9_-]{4,40}$", ErrorMessage = "A becenév formátuma nem megfelelő.")]
        public String NickName { get; set; }

        [Display(Name = "Email cím")]
        [Required(ErrorMessage = "Az email cím mezőjét kötelező kitölteni.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Nem megfelelő email formátum.")]
        public String EmailAddress { get; set; }

        [Display(Name = "Email cím megerősítése")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Nem megfelelő email formátum.")]
        [Compare(nameof(EmailAddress), ErrorMessage = "Az email címek nem egyeznek.")]
        public String ValidateEmailAddress { get; set; }

        [Display(Name = "Jelszó (Minimum 8, maximum 40 karakter hosszú lehet, csak betűt, számot és kötőjelet tartalmazhat.)")]
        [Required(ErrorMessage = "A jelszó mezőt kötelező kitölteni.")]
        [RegularExpression("^[A-Za-z0-9_-]{8,40}$", ErrorMessage = "A jelszó formátuma nem megfelelő.")]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        [Display(Name = "Jelszó megerősítése")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "A jelszavak nem egyeznek.")]
        public String ValidatePassword { get; set; }
    }
}
