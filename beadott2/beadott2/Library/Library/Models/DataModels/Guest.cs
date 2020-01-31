using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Library.Models
{
    public class Guest : IdentityUser<int>
    {
        //public int ID { get; set; }
        public String Name { get; set; }
        public String Address { get; set; }        
        //public override String PhoneNumber { get; set; }

        public virtual ICollection<Lending> Lendings { get; set; }
    }
}
