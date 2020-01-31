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
using Library.Contexts;
using Library.Controllers;
using Microsoft.AspNetCore.Identity;

namespace Library.Models
{
    // Used for seeding the database to able testing.
    public class SeedData
    {

        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new LibraryContext(
                serviceProvider.GetRequiredService<DbContextOptions<LibraryContext>>()))
            {
                // Only used for debugging.
                //DropDatabase(context);
                //return;
                
                context.Database.Migrate();

                // Look for any books.
                if (context.Book.Any())
                {
                    
                    return; // DB has been seeded
                }

                SeedBooks(context);
                SeedVol(context);
                SeedBookImage(context, "BookImages");
                SeedLending(context);               
            }
        }

        // Helper function: if need to delete database before refreshing.
        public static void DropDatabase(LibraryContext context)
        {
            context.Database.EnsureDeleted();

        }

        public static void SeedBooks(LibraryContext context)
        {
            // Add a few real book.
            context.Book.AddRange(
                new Book
                {
                    Title = "Lucas",
                    Author = "Kevin Brooks",
                    ReleaseYear = 2007,
                    ISBN = 1,
                },

                new Book
                {
                    Title = "Ballagó idő",
                    Author = "Fekete István",
                    ReleaseYear = 1995,
                    ISBN = 2,
                },

                new Book
                {
                    Title = "Esti Kornél",
                    Author = "Kosztolányi Dezső",
                    ReleaseYear = 1888,
                    ISBN = 3,
                },

                new Book
                {
                    Title = "Édes Anna",
                    Author = "Kosztolányi Dezső",
                    ReleaseYear = 1888,
                    ISBN = 4,
                }
            );

            // Generate some more books for testing.
            List<Book> books = new List<Book>();
            String[] authorsSecondName =
            {
                "Magda", "Anna", "Miklós", "István", "György",
                "Magdaléna", "Elek", "Ubul", "Oszkár", "Annika"
            };
            String[] authorsFirstName =
            {
                "Szabó", "Kovács", "Fekete", "Benedek", "Asztalos",
                "Lakatos", "Dragomán", "Nemere", "Kosztolányi", "Fóti"
            };
            int[] releaseYears = 
            {
                2007,1995, 1856, 1234, 2012,
                1998, 1996, 1994, 2019, 2018
            };
            String[] titleFirstPart =
            {
                "Egy író", "Egy vándor", "Egy éjszakai pillangó", "Bandika", "Ő",

                "Egy tucat jelentéktelen alak", "A barna lány", "Az elmeháborodott", "Józsi", "Egy senki"
            };
            String[] titleLastPart = 
            {
                "bolyong", "szenved", "keres egy fél macifület", "vergődik", "szeret",
                "megbékél", "kéreget", "világgá megy", "kiszenved", "varázsol"
            };
            String[] titleMiddlePart = 
            {
                "nyugodtan", "szerelmével", "viharban", "tűzben", "bűnben",
                "szándékosan", "vérszomlyasan", "egy bohóccal", "egy félnótással", "reménytelenül"
            };
            int getISBN = 5;

            Random rnd = new Random();
   
            for(int i = 0; i < 30; i++)
            {
                context.Book.Add(
                new Book
                {
                    Title = titleFirstPart[rnd.Next(0,10)] + " " + titleMiddlePart[rnd.Next(0,10)] + " " + titleLastPart[rnd.Next(0,10)],
                    Author = authorsFirstName[rnd.Next(0,10)] + " " + authorsSecondName[rnd.Next(0,10)],
                    ReleaseYear = releaseYears[rnd.Next(0,10)],
                    ISBN = getISBN
                });
                getISBN++;
            }
     
            context.SaveChanges();
        }
        

        public static void SeedVol(LibraryContext context)
        {
            int volID = 1;
            bool switchVolCount = true;
            foreach(var book in context.Book)
            {
                context.Vol.Add(
                    new Vol
                    {
                        Book = book,
                    });
                volID++;
                if (switchVolCount)
                {
                    context.Vol.Add(
                    new Vol
                    {
                        Book = book,
                    });
                    volID++;
                    switchVolCount = false;
                }
                else
                {
                    switchVolCount = true;
                }
            }
            
            context.SaveChanges();
        }

        public static void SeedBookImage(LibraryContext context, String imageDirectory)
        {

            // Seed book with id 1.
            var bookISBN = 1;
            var imagePath = Path.Combine(imageDirectory, "kevin_brooks_lucas.jpg");
            if (File.Exists(imagePath))
            {
                context.BookImage.Add(new BookImage
                {
                    Image = File.ReadAllBytes(imagePath),
                    BookISBN = bookISBN
                });
            }

            Trace.WriteLine("\n" + bookISBN + "\n");

            // Seed book with id 3.
            bookISBN = 3;
            imagePath = Path.Combine(imageDirectory, "kosztolanyi_esti_kornel.jpg");
            if (File.Exists(imagePath))
            {
                context.BookImage.Add(new BookImage
                {
                    Image = File.ReadAllBytes(imagePath),
                    BookISBN = bookISBN
                });
            }
            Trace.WriteLine("\n" + bookISBN + "\n");

            // Seed book with id 4.
            bookISBN = 4;
            imagePath = Path.Combine(imageDirectory, "kosztolanyi_edes_anna.jpg");
            if (File.Exists(imagePath))
            {
                context.BookImage.Add(new BookImage
                {
                    Image = File.ReadAllBytes(imagePath),
                    BookISBN = bookISBN
                });
            }
            Trace.WriteLine("\n" + bookISBN + "\n");

            context.SaveChanges();
        }

        public static void SeedLending(LibraryContext context)
        {
           /* context.Lending.AddRange(
                new Lending
                {
                    Vol = context.Vol.Where(b => b.ID == 1).FirstOrDefault(),
                    Guest = context.Guest.Where(g => g.GuestID == 1).FirstOrDefault(),
                    StartDay = DateTime.Parse("2019-3-25"),
                    EndDay = DateTime.Parse("2019-4-25"),
                    IsActive = false
                },
                new Lending
                {
                    Vol = context.Vol.Where(b => b.ID == 2).FirstOrDefault(),
                    Guest = context.Guest.Where(g => g.GuestID == 1).FirstOrDefault(),
                    StartDay = DateTime.Parse("2019-3-20"),
                    EndDay = DateTime.Parse("2019-4-20"),
                    IsActive = false
                },
                new Lending
                {
                    Vol = context.Vol.Where(b => b.ID == 5).FirstOrDefault(),
                    Guest = context.Guest.Where(g => g.GuestID == 2).FirstOrDefault(),
                    StartDay = DateTime.Parse("2019-3-11"),
                    EndDay = DateTime.Parse("2019-4-11"),
                    IsActive = false
                },

                new Lending
                {
                    Vol = context.Vol.Where(b => b.ID == 5).FirstOrDefault(),
                    Guest = context.Guest.Where(g => g.GuestID == 2).FirstOrDefault(),
                    StartDay = DateTime.Parse("2019-4-12"),
                    EndDay = DateTime.Parse("2019-4-20"),
                    IsActive = false
                },


                new Lending
                {
                    Vol = context.Vol.Where(b => b.ID == 5).FirstOrDefault(),
                    Guest = context.Guest.Where(g => g.GuestID == 2).FirstOrDefault(),
                    StartDay = DateTime.Parse("2019-4-21"),
                    EndDay = DateTime.Parse("2019-4-30"),
                    IsActive = false
                }

                );

            context.SaveChanges();*/
        }
        
    }
}
