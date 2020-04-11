using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models
{
    public class GameResultViewModel
    {
        public String GameTitle { get; set; }

        [Display(Name ="Előtörténet")]
        public String Prelude { get; set; }

        [Display(Name ="A győzelmet követően megjelenő szöveg")]
        public String GameWonResult { get; set; }

        [Display(Name = "A vereséget követően megjelenő szöveg")]
        public String GameLostResult { get; set; }

    }
}
