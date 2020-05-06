using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using AdventureGameEditor.Models.Enums;

namespace AdventureGameEditor.Models.ViewModels.GameEditor
{
    public class FieldDetailsViewModel
    {
        [Display(Name ="Oszlopszám")]
        public int ColNumber { get; set; }

        [Display(Name = "Sorszám")]
        public int RowNumber { get; set; }

        [Display(Name = "A mező szövege")]
        public String TextContent { get; set; }

        [Display(Name ="A mező illusztrációja")]
        public int PictureID { get; set; }

        // Describes if this filed have a trial (true) or not (false).
        public Boolean IsTrial { get; set; }

        [Display(Name = "Próba típusa")]
        public TrialType TrialType { get; set; }

        [Display(Name = "alternatíva szövege")]
        public List<String> AlternativeTexts { get; set; }

        [Display(Name = "alternatíva eredménye")]
        public List<TrialResult> TrialResults { get; set; }

        public String TrialText { get; set; }
    }
}
