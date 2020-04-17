using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AdventureGameEditor.Models
{
    public class GameResultViewModel
    {
        public String GameTitle { get; set; }

        [Display(Name = "Előtörténet")]
        [Required(ErrorMessage = "Az előtörténet mezője nem lehet üres.")]
        public String Prelude { get; set; }

        [Display(Name ="Az előtörténethez tartozó kép")]
        [BindProperty]
        public IFormFile PreludeImage { get; set; }

        [Display(Name = "A győzelmet követően megjelenő szöveg")]
        [Required(ErrorMessage = "A győzelmet követően megjelenő szöveg mezője nem lehet üres.")]
        public String GameWonResult { get; set; }

        [Display(Name = "A győzelmet követően megjelenő kép")]
        [BindProperty]
        public IFormFile GameWonImage { get; set; }

        [Display(Name = "A vereséget követően megjelenő szöveg")]
        [Required(ErrorMessage = "A vereséget követően megjelenő szöveg mezője nem lehet üres.")]
        public String GameLostResult { get; set; }

        [Display(Name = "A vereséget követően megjelenő kép")]
        [BindProperty]
        public IFormFile GameLostImage { get; set; }

        public List<String> ErrorMessages { get; set; }
 

    }
}
