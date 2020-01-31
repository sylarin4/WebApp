using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Library.Models;

namespace Library.Contexts
{
    public class LibrarianContext : IdentityDbContext<Guest, IdentityRole<int>, int>
    {
        public LibrarianContext (DbContextOptions<LibrarianContext> options)
            : base(options)
        {
        }

        public DbSet<Library.Models.Librarian> Librarian { get; set; }
    }
}
