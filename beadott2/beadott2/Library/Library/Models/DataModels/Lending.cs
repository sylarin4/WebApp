using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Library.Models
{
    public class Lending
    {
        public int ID { get; set; }
        public Vol Vol { get; set; }
        public Guest Guest { get; set; }

        [Required(ErrorMessage = "Start day is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDay { get; set; }

        [Required(ErrorMessage = "End day is required.")]
        [DataType(DataType.Date)]
        public DateTime EndDay { get; set; }

        public bool IsActive { get; set; }
    }
}
