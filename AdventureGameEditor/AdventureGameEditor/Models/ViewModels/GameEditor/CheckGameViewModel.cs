using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models.ViewModels.GameEditor
{
    public class CheckGameViewModel
    {
        public String GameTitle { get; set; }
        [Display(Name ="A térkép valid: ")]
        public Boolean IsMapValid { get; set; }

        [Display(Name = "Valid kezdőmező van beállítva: ")]
        public Boolean IsStartFieldSet { get; set; }

        [Display(Name = "Valid célmező van beállítva: ")]
        public Boolean IsTargetFieldSet { get; set; }

        [Display(Name = "Az előtörténet ki van töltve: ")]
        public Boolean IsPreludeFilled { get; set; }

        [Display(Name = "A győzelmet követően megjelenő szöveg ki van töltve: ")]
        public Boolean IsGameWonFilled { get; set; }

        [Display(Name = "A vereséget követően megjelenő szöveg ki van töltve: ")]
        public Boolean IsGameLostFilled { get; set; }

        [Display(Name ="A kezdőmezőből elérhető a célmező:")]
        public Boolean IsSolutionExists { get; set; }

        [Display(Name ="A legrövidebb út hossza a kezdőmezőtől a célmezőig:")]
        public int PathLenght { get; set; }
    }
}
