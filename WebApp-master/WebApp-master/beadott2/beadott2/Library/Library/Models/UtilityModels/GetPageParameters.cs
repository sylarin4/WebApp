using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models
{
    public class GetPageParameters
    {
        public int PageNumber { get; set; }
        public bool IsDefaultOrdered { get; set; }
    }
}
