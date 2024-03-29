﻿using System;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models.ViewModels.UserAutentication
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
