using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

using AdventureGameEditor.Data;

namespace AdventureGameEditor.Models
{
    public class SeedTestData
    {

        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new AdventureGameEditorContext(
                serviceProvider.GetRequiredService<DbContextOptions<AdventureGameEditorContext>>()))
            {
                // Only used for debugging.
                //DropDatabase(context);
                //return;

                context.Database.Migrate();

                // Look for any books.
                if (context.User.Any())
                {

                    return; // DB has been seeded
                }

                SeedUsers(context);
            }
        }

        // Helper function: if need to delete database before refreshing.
        public static void DropDatabase(AdventureGameEditorContext context)
        {
            context.Database.EnsureDeleted();

        }

        public static void SeedUsers(AdventureGameEditorContext context)
        {
        }

    }
}
