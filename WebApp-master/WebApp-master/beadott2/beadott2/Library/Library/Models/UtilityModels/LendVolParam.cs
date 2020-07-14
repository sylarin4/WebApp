using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models
{
    // Utility model of creating new lending. Used for pass information about the
    // volume to lend and the guest who make it to the Lendings' Create view.
    public class LendVolParam
    {
        public String UserName { get; set; }
        public int VolID { get; set; }
    }
}
