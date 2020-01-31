using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Data
{
    public class VolDTO
    {
        public int ID { get; set; }
        public int BookID { get; set; }
        public int VolID { get; set; }

        public virtual IEnumerable<LendingDTO> Lendings { get; set; }
    }
}
