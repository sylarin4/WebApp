using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using AdventureGameEditor.Models.Enums;

namespace AdventureGameEditor.Models.ViewModels.GameEditor
{
    public class BasicGameDataViewModel
    {
        [Display(Name = "A játék címe")]
        [Required(ErrorMessage = "A játék cím mezője nem lehet üres.")]
        [StringLength(40, MinimumLength = 7, 
            ErrorMessage ="A játék címének legalább 7, és maximum 40 karakter hosszúnak kell lennie!")]
        public String Title { get; set; }
        [Display(Name ="Ki láthatja a játékot?")]
        public Visibility Visibility { get; set; }

        [Display(Name = "A játék térképének mérete")]
        [Required(ErrorMessage = "A térkép méretének mezője nem lehet üres.")]
        [Range(3, 15, ErrorMessage = "3 és 15 között kell lennie a térkép méretének.")]
        public int TableSize { get; set; }

        [Display(Name = "Borítókép")]
        [BindProperty]
        public IFormFile CoverImage { get; set; }

    }
}
