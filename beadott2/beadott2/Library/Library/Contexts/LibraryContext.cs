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
    public class LibraryContext : IdentityDbContext<Guest, IdentityRole<int>, int>
    {
        public LibraryContext (DbContextOptions<LibraryContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Guest>().ToTable("Guest");
        }

        public DbSet<Library.Models.Book> Book { get; set; }

        public DbSet<Library.Models.Lending> Lending { get; set; }

        public DbSet<Library.Models.Vol> Vol { get; set; }

        public DbSet<Library.Models.BookImage> BookImage { get; set; }
    }
}
