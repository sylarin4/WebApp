using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdventureGameEditor.Models
{
    public class FieldTextAndImageContentViewModel
    {
        public String GameTitle { get; set; }
        public int ColNumber { get; set; }
        public int RowNumber { get; set; }

        [Display(Name = "Az előtörténethez tartozó kép")]
        [BindProperty]
        public IFormFile NewImage { get; set; }
        public int? CurrentImageID { get; set; }

        [Display(Name = "A mező szövege")]
        public String TextContent { get; set; }

    }
}
