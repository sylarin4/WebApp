﻿using System;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models.ViewModels.UserAutentication
{
    public class ProfileViewModel
    {
        [Display(Name = "Felhasználó név")]
        public String UserName { get; set; }

        [Display(Name = "Becenév")]
        public String NickName { get; set; }

        [Display(Name = "Email cím")]
        public String EmailAddress { get; set; }
    }
}
