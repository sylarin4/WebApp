using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models
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
        public String Text { get; set; }
    }
}
