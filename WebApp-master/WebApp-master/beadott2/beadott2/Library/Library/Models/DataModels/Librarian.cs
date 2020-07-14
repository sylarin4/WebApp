using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Library.Models
{
    public class Librarian : IdentityUser<int>
    {
        public String Name { get; set; }
        public int LibrarianID { get; set; }
    }
}
