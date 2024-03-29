﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using AdventureGameEditor.Models.Enums;
using AdventureGameEditor.Models.DatabaseModels.Game;

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
