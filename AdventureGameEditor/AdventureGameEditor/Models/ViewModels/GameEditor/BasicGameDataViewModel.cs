using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models
{
    public class BasicGameDataViewModel
    {
        [Display(Name = "A játék címe")]
        [Required(ErrorMessage = "A játék cím mezője nem lehet üres.")]
        public String Title { get; set; }
        [Display(Name ="Ki láthatja a játékot?")]
        public Visibility Visibility { get; set; }

        [Display(Name = "A játék térképének mérete")]
        [Required(ErrorMessage = "A játék térkép méretének mezője nem lehet üres.")]
        [Range(3, 15, ErrorMessage = "3 és 15 között kell lennie a térkép méretének.")]
        public int TableSize { get; set; }
    }
}
