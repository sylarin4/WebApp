using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using AdventureGameEditor.Models.Enums;

namespace AdventureGameEditor.Models.ViewModels.GameEditor
{
    public class FieldContentViewModel
    {
        public String GameTitle { get; set; }
        public int ColNumber { get; set; }
        public int RowNumber { get; set; }

        [Display(Name = "A mező szövege")]
        public String TextContent { get; set; }
        public List<String> AlternativeTexts { get; set; }
        public List<TrialResult> TrialResults { get; set; }
        public TrialType TrialType { get; set; }

    }
}
