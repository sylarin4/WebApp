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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>().ToTable("User");
        }

        public DbSet<AdventureGameEditor.Models.User> User { get; set; }

        public DbSet<AdventureGameEditor.Models.DatabaseModels.Game.Game> Game { get; set; }

        public DbSet<AdventureGameEditor.Models.DatabaseModels.MapImage> MapImage { get; set; }

        public DbSet<AdventureGameEditor.Models.DatabaseModels.Game.Field> Field { get; set; }

        public DbSet<AdventureGameEditor.Models.GameplayData> GameplayData { get; set; }

        public DbSet<AdventureGameEditor.Models.DatabaseModels.Game.GameResult> GameResult { get; set; }

        //public DbSet<AdventureGameEditor.Models.Feedback> Feedback { get; set; }
    }
}
