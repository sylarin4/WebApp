using System;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Library.Contexts;
using Library.Data;
using Library.Models;

namespace Library.Test
{
    public class LibraryTest : IDisposable
    {
        private readonly LibraryContext _context;
        private readonly List<BookDTO> _bookDTOs;
        private readonly List<VolDTO> _volDTOs;
        private readonly List<LendingDTO> _lendingDTOs;
        private Guest _testGuest;



        public LibraryTest()
        {
            // Initial LibraryCobntext.
            var libraryContextOptions = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase("LibraryTest")
                .Options;

            _context = new LibraryContext(libraryContextOptions);
            _context.Database.EnsureCreated();

            _testGuest = new Guest
            {
                Id = 0,
                Name = "TestGuest",
                Address = "TestGuestAddress",
                PhoneNumber = "01234557"
            };

            var bookData = new List<Book>
            {
                new Book
                {
                    Title = "TestTitle1",
                    Author = "TestAuthor1",
                    ReleaseYear = 1,
                    ISBN= 1
                },
                new Book
                {
                    Title = "TestTitle2",
                    Author = "TestAuthor2",
                    ReleaseYear = 2,
                    ISBN = 2
                },
                new Book
                {
                    Title = "TestTitle3",
                    Author = "TestAuthor3",
                    ReleaseYear = 3,
                    ISBN = 3
                }
            };
            foreach (var book in bookData)
            {
                _context.Book.Add(book);
            }

            var volData = new List<Vol>
            {
               new Vol{ Book = bookData.Where(b => b.ISBN == 1).FirstOrDefault()},
               new Vol{ Book = bookData.Where(b => b.ISBN == 2).FirstOrDefault()},
               new Vol{ Book = bookData.Where(b => b.ISBN == 2).FirstOrDefault()},
               new Vol{ Book = bookData.Where(b => b.ISBN == 3).FirstOrDefault()},
               new Vol{ Book = bookData.Where(b => b.ISBN == 3).FirstOrDefault()},
               new Vol{ Book = bookData.Where(b => b.ISBN == 3).FirstOrDefault()}
            };
            foreach (var vol in volData)
            {
                _context.Vol.Add(vol);
            }
            _context.SaveChanges();

            var lendingData = new List<Lending>
            {
                new Lending
                {
                    ID = 1,
                    Vol = _context.Vol.Include(v => v.Book).Where(v => v.Book.ISBN == 1).FirstOrDefault(),
                    Guest = _testGuest,
                    StartDay = new DateTime(2019, 6, 1),
                    EndDay = new DateTime(2019, 6, 10),
                    IsActive = false
                },
                new Lending
                {
                    ID = 2,
                    Vol =  _context.Vol.Include(v => v.Book).Where(v => v.Book.ISBN == 2).FirstOrDefault(),
                    Guest = _testGuest,
                    StartDay = new DateTime(2019, 5, 25),
                    EndDay = new DateTime(2019, 6, 10),
                    IsActive = true
                }

            };

            _context.Lending.AddRange(lendingData);
            _context.SaveChanges();

            _bookDTOs = _context.Book.Include(b => b.Vols).Select(book => new BookDTO
            {
                ID = book.ID,
                ISBN = book.ISBN,
                Author = book.Author,
                Title = book.Title,
                ReleaseYear = book.ReleaseYear,
                VolIDs = book.Vols.Select(v => v.ID).ToList()

            }).ToList();

            _lendingDTOs = lendingData.Select(lending => new LendingDTO
            {
                ID = lending.ID,
                VolID = lending.Vol.ID,
                GuestID = lending.Guest.Id,
                StartDay = lending.StartDay,
                EndDay = lending.EndDay,
                IsActive = lending.IsActive
            }).ToList();

            _volDTOs = _context.Vol.Select(vol => new VolDTO
            {
                ID = vol.ID,
                BookID = vol.Book.ID,
                VolID = vol.ID,
                Lendings = _lendingDTOs.Where(l => l.VolID == vol.ID).ToList()

            }).ToList();

        }
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void ListBooksTest()
        {
            var controller = new AdminController(_context);
            var result = controller.ListBooks();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<BookDTO>>(objectResult.Value).ToList();

            for (int i = 0; i < _bookDTOs.Count(); ++i)
            {
                Assert.Equal(_bookDTOs[i].ID, model[i].ID);
                Assert.Equal(_bookDTOs[i].ISBN, model[i].ISBN);
                Assert.Equal(_bookDTOs[i].Author, model[i].Author);
                Assert.Equal(_bookDTOs[i].Title, model[i].Title);
                Assert.Equal(_bookDTOs[i].ReleaseYear, model[i].ReleaseYear);
                for (int j = 0; j < model[i].VolIDs.Count; ++j)
                {
                    Assert.Equal(_bookDTOs[i].VolIDs[j], model[i].VolIDs[j]);
                }
            }
        }

        [Fact]
        public void ListLendingsTest()
        {
            var controller = new AdminController(_context);
            var result = controller.ListLendings();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<LendingDTO>>(objectResult.Value).ToList();

            for (int i = 0; i < _lendingDTOs.Count(); ++i)
            {
                Assert.Equal(_lendingDTOs[i].ID, model[i].ID);
                Assert.Equal(_lendingDTOs[i].VolID, model[i].VolID);
                Assert.Equal(_lendingDTOs[i].GuestID, model[i].GuestID);
                Assert.Equal(_lendingDTOs[i].StartDay, model[i].StartDay);
                Assert.Equal(_lendingDTOs[i].EndDay, model[i].EndDay);
                Assert.Equal(_lendingDTOs[i].IsActive, model[i].IsActive);
            }
        }

        [Fact]
        public void DeleteVolTestWithOneVol()
        {
            var controller = new AdminController(_context);
            int countBefore = _context.Vol.Count();

            int deleteID = _context.Vol.Include(v => v.Book)
                                       .Where(v => v.Book.ISBN == 1)
                                       .Select(v => v.ID)
                                       .FirstOrDefault();

            var result = controller.DeleteVol(deleteID);

            Assert.Empty(_context.Vol.Where(v => v.ID == deleteID));
            Assert.Equal(countBefore - 1, _context.Vol.Count());

        }

        [Fact]
        public void DeleteVolTestWithTwoVols()
        {
            var controller = new AdminController(_context);
            int countBefore = _context.Vol.Count();
            int deleteId1 = _context.Vol.Include(v => v.Book)
                                        .Where(v => v.Book.ISBN == 3)
                                        .Select(v => v.ID)
                                        .FirstOrDefault();

            var result = controller.DeleteVol(deleteId1);

            Assert.Empty(_context.Vol.Where(v => v.ID == deleteId1));
            Assert.Equal(countBefore - 1, _context.Vol.Count());

            int deleteId2 = _context.Vol.Include(v => v.Book)
                                    .Where(v => v.Book.ISBN == 3)
                                    .Select(v => v.ID)
                                    .FirstOrDefault();

            result = controller.DeleteVol(deleteId2);

            Assert.Empty(_context.Vol.Where(v => v.ID == deleteId2));
            Assert.Equal(countBefore - 2, _context.Vol.Count());
        }

        [Fact]
        public void AddOneBookTest()
        {
            var testBook = new BookDTO
            {
                Title = "AddedBookTitle",
                Author = "AddedBookAuthor",
                ReleaseYear = 2019,
                ISBN = 999
            };

            var controller = new AdminController(_context);
            var countBefore = _context.Book.Count();

            var result = controller.AddBook(testBook);
            
            var resultBook = _context.Book.Where(b => b.ISBN == 999).FirstOrDefault();
            Assert.NotNull(resultBook);
            Assert.Equal(resultBook.Title, testBook.Title);
            Assert.Equal(resultBook.Author, testBook.Author);
            Assert.Equal(resultBook.ReleaseYear, testBook.ReleaseYear);
            Assert.Equal(resultBook.ISBN, testBook.ISBN);
            Assert.Equal(countBefore + 1, _context.Book.Count());
        }

        [Fact]
        public void AddTwoBookTest()
        {
            var testBook1 = new BookDTO
            {
                ISBN = 111,
                Author = "AddedTestBookAuthor1",
                Title = "AddedTestBookTitle1",
                ReleaseYear = 1111
            };
            var testBook2 = new BookDTO
            {
                ISBN = 222,
                Author = "AddedTestBookAuthor2",
                Title = "AddedTestBookTitle2",
                ReleaseYear = 2222
            };
            var controller = new AdminController(_context);
            int countBefore = _context.Book.Count();

            var response = controller.AddBook(testBook1);

            var resultBook = _context.Book.Where(b => b.ISBN == 111).FirstOrDefault();
            Assert.Equal(testBook1.ISBN, resultBook.ISBN);
            Assert.Equal(testBook1.Author, resultBook.Author);
            Assert.Equal(testBook1.Title, resultBook.Title);
            Assert.Equal(testBook1.ReleaseYear, resultBook.ReleaseYear);
            Assert.Equal(countBefore + 1, _context.Book.Count());

            response = controller.AddBook(testBook2);

            resultBook = _context.Book.Where(b => b.ISBN == 222).FirstOrDefault();
            Assert.Equal(testBook2.ISBN, resultBook.ISBN);
            Assert.Equal(testBook2.Author, resultBook.Author);
            Assert.Equal(testBook2.Title, resultBook.Title);
            Assert.Equal(testBook2.ReleaseYear, resultBook.ReleaseYear);
            Assert.Equal(countBefore + 2, _context.Book.Count());
        }

        [Fact]
        public void AddOneVolTest()
        {
            int testBookId = _context.Book.Where(b => b.ISBN == 2).Select(b => b.ID).FirstOrDefault();
            var controller = new AdminController(_context);
            var countBefore = _context.Vol.Count();

            var result = controller.AddVol(testBookId);

            Assert.NotNull(_context.Vol.Include(v => v.Book).Where(v => v.Book.ID == testBookId));
            Assert.Equal(countBefore + 1, _context.Vol.Count());
        }

        [Fact]
        public void AddTwoVolTest()
        {
            int testBookId1 = _context.Book.Where(b => b.ISBN == 1).Select(b => b.ID).FirstOrDefault();
            var controller = new AdminController(_context);
            var countBefore = _context.Vol.Count();

            var result = controller.AddVol(testBookId1);

            Assert.NotNull(_context.Vol.Include(v => v.Book).Where(v => v.Book.ID == testBookId1));
            Assert.Equal(countBefore + 1, _context.Vol.Count());

            result = controller.AddVol(testBookId1);

            Assert.NotNull(_context.Vol.Include(v => v.Book).Where(v => v.Book.ID == testBookId1));
            Assert.Equal(countBefore + 2, _context.Vol.Count());

        }

        [Fact]
        public void ActivateLendingTest()
        {
            var controller = new AdminController(_context);

            var result = controller.ActivateLending(1);
            result = controller.ActivateLending(2);

            Assert.True(_context.Lending.Where(l => l.ID == 1).Select(l => l.IsActive).FirstOrDefault());
            Assert.True(_context.Lending.Where(l => l.ID == 2).Select(l => l.IsActive).FirstOrDefault());
        }

        [Fact]
        public void InactivateLendingTest()
        {
            var controller = new AdminController(_context);
            var result = controller.InactivateLending(1);

            result = controller.InactivateLending(2);

            // Assert.
            Assert.False(_context.Lending.Where(l => l.ID == 1).Select(l => l.IsActive).FirstOrDefault());
            Assert.False(_context.Lending.Where(l => l.ID == 2).Select(l => l.IsActive).FirstOrDefault());
        }

        [Fact]
        public void ActivateAndInactivateLendingTest()
        {
            var controller = new AdminController(_context);

            // Activate lending.
            var response = controller.ActivateLending(2);
            Assert.True(_context.Lending.Where(l => l.ID == 2).Select(l => l.IsActive).FirstOrDefault());

            // Inactivate lending.
            response = controller.InactivateLending(2);
            Assert.False(_context.Lending.Where(l => l.ID == 2).Select(l => l.IsActive).FirstOrDefault());

            // Activate lending again.
            response = controller.ActivateLending(2);
            Assert.True(_context.Lending.Where(l => l.ID == 2).Select(l => l.IsActive).FirstOrDefault());
        }
    }
}
