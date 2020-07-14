using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Data
{
    public class LendingDTO
    {
        public int ID { get; set; }
        public int VolID { get; set; }
        public int GuestID { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
        public bool IsActive { get; set; }
    }
}
