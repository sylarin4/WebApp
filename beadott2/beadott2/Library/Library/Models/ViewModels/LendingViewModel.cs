using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Library.Models
{
    // Model of Lengings' Create view. Used for creat new lendings.
    public class LendingViewModel
    {

        [HiddenInput(DisplayValue = false)]
        public int VolID { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int GuestID { get; set; }

        [Required(ErrorMessage = "Start day is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDay { get; set; }

        [Required(ErrorMessage = "End day is required.")]
        [DataType(DataType.Date)]
        public DateTime EndDay { get; set; }
    }
}
