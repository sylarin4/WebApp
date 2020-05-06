using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models.ViewModels.GameEditor
{
    public class FieldTrialContentViewModel
    {
        public String GameTitle { get; set; }
        public int ColNumber { get; set; }
        public int RowNumber { get; set; }
        public List<String> AlternativeTexts { get; set; }
        public List<TrialResult> TrialResults { get; set; }

        [Display(Name ="Próba típusa")]
        public TrialType TrialType { get; set; }

        [Display(Name ="Próba szövege")]
        [Required(ErrorMessage ="A szövegmező nem hagyható üresen.")]
        public String Text { get; set; }
    }
}
