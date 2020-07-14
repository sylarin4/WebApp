using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AdventureGameEditor.Models.ViewModels.GameEditor
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

        // If we editing the game, maybe we've already upload and image. If we have, show it.
        [Display(Name ="Korábban feltöltött kép:")]
        public int PreludeImageID { get; set; }

        [Display(Name = "A győzelmet követően megjelenő szöveg")]
        [Required(ErrorMessage = "A győzelmet követően megjelenő szöveg mezője nem lehet üres.")]
        public String GameWonResult { get; set; }

        [Display(Name = "A győzelmet követően megjelenő kép")]
        [BindProperty]
        public IFormFile GameWonImage { get; set; }

        // When we editing the game, show if we already uploaded an image.
        [Display(Name ="Korábban feltöltött kép: ")]
        public int GameWonImageID { get; set; }

        [Display(Name = "A vereséget követően megjelenő szöveg")]
        [Required(ErrorMessage = "A vereséget követően megjelenő szöveg mezője nem lehet üres.")]
        public String GameLostResult { get; set; }

        [Display(Name = "A vereséget követően megjelenő kép")]
        [BindProperty]
        public IFormFile GameLostImage { get; set; }

        // When we editing the game and loaded image in the past, show the uploaded image.
        [Display(Name ="Korábban feltöltött kép:")]
        public int GameLostImageID { get; set; }

        [Display(Name ="A játék történetének rövid ismertetője:")]
        public String Summary { get; set; }

        [Display(Name = "Új borítókép feltöltése")]
        [BindProperty]
        public IFormFile NewCoverImage { get; set; }

        public List<String> ErrorMessages { get; set; }
 
        
    }
}
