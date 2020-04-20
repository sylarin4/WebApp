using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models
{
    public class CheckGameViewModel
    {
        public String GameTitle { get; set; }
        [Display(Name ="A térkép valid: ")]
        public Boolean IsMapValid { get; set; }

        [Display(Name = "A kezdőmező be van állítva: ")]
        public Boolean IsStartFieldSet { get; set; }

        [Display(Name = "A célmező be van állítva: ")]
        public Boolean IsTargetFieldSet { get; set; }

        [Display(Name = "Az előtörténet ki van töltve: ")]
        public Boolean IsPreludeFilled { get; set; }

        [Display(Name = "A győzelmet követően megjelenő szöveg ki van töltve: ")]
        public Boolean IsGameWonFilled { get; set; }

        [Display(Name = "A vereséget követően megjelenő szöveg ki van töltve: ")]
        public Boolean IsGameLostFilled { get; set; }
    }
}
