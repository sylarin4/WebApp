using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AdventureGameEditor.Models;

namespace AdventureGameEditor.Data
{
    public class AdventureGameEditorContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AdventureGameEditorContext (DbContextOptions<AdventureGameEditorContext> options)
            : base(options)
        {
        }

        public DbSet<AdventureGameEditor.Models.User> User { get; set; }
    }
}
