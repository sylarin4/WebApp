using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models
{
    // Utility model. Used by Guests' RegisterResult view to show the just registered
    // guest's data.
    public class RegisterResult
    {
        public String Name { get; set; }
        public String Address { get; set; }
        public String PhoneNumber { get; set; }

        public bool IsSuccess { get; set; } 
    }
}
