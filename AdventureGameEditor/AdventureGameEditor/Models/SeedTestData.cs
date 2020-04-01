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

                // Look for any users.
                if (context.MapImage.Any())
                {

                    return; // DB has been seeded
                }

                SeedUsers(context);
                SeedMapImages(context);
            }
        }

        // Helper function: if need to delete database before refreshing.
        public static void DropDatabase(AdventureGameEditorContext context)
        {
            context.Database.EnsureDeleted();

        }

        // TODO: implement or delete it.
        public static void SeedUsers(AdventureGameEditorContext context)
        {
        }

        public static void SeedMapImages(AdventureGameEditorContext context)
        {
            foreach (String filePath in Directory.GetFiles("Pictures/MapPictures"))
            {
                // TODO: make it more effective
                int wayDirectionsCode = 0;
                for(int i = 0; i < 4; ++i)
                {
                    if(filePath[(filePath.Length) -5 -i] == '1')
                    {
                        wayDirectionsCode += (int)Math.Pow(10, i);
                    }
                }
                
                context.MapImage.Add(
                    new MapImage
                {
                    Image = File.ReadAllBytes(filePath),
                    Theme = MapTheme.Test,
                    WayDirectionsCode = wayDirectionsCode
                    
                });
            }
            context.SaveChanges();
        }

    }
}
