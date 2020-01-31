using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Newtonsoft.Json;
using Library.Contexts;
using Library.Data;

namespace Library.Models
{
    public class LibraryService : ILibraryService
    {
        private readonly LibraryContext _context;

        public LibraryService(LibraryContext context)
        {
            _context = context;
        }


        #region Listing methods 

        public IEnumerable<Book> ListBooks()
        {
            return  _context.Book.Include(b => b.Vols)
                            .ThenInclude(b => b.Lendings)
                            .OrderByDescending(b => b.Vols.SelectMany(v => v.Lendings).Count())
                            .ToList();
        }

        public IEnumerable<Lending> ListLendings()
        {
            return _context.Lending.Include(l => l.Guest).Include(l => l.Vol).ToList();
        }

        public List<int> GetVolsByBookId(int BookID)
        {
            return _context.Vol.Where(v => v.Book.ID == BookID)
                               .Select(v => v.ID)
                               .ToList();
        }

        // List the given page of books by given order.
        public BooksIndexModel OrderBooks(int pageNumber, bool isOrderByPopularity)
        {
            if (isOrderByPopularity)
            {
                return new BooksIndexModel
                {
                    Books = _context.Book.Include(b => b.Vols)
                                                .ThenInclude(z => z.Lendings)
                                                .OrderByDescending(b => b.Vols.SelectMany(v => v.Lendings).Count())
                                                .ToList()
                                                .Skip((pageNumber - 1) * 20)
                                                .Take(20),
                    IsDefaultOrdered = true,
                    PageNumber = pageNumber
                };
            }
            else
            {
                return new BooksIndexModel
                {
                    Books = _context.Book.Include(b => b.Vols)
                                .OrderBy(b => b.Title)
                                .ToList()
                                .Skip((pageNumber - 1) * 20)
                                .Take(20),
                    IsDefaultOrdered = false,
                    PageNumber = pageNumber
                };
            }
        }

        public BooksIndexModel GetPageBooks(String serializedParam, bool isNextPage)
        {
            // Deserialize parameters.
            GetPageParameters param = JsonConvert.DeserializeObject<GetPageParameters>(serializedParam);

            // If want to get the next page and the current page is the last not able
            // to get next one. 
            // Or if want to get the previous page and current page is the first not 
            // able to get the previous one.
            if (
                (isNextPage && (_context.Book.Count() <= param.PageNumber * 20)) ||
                (!isNextPage && param.PageNumber < 2)
                )
            {
                return OrderBooks(param.PageNumber, param.IsDefaultOrdered);
            }

            if (isNextPage)
            {
                return OrderBooks(param.PageNumber + 1, param.IsDefaultOrdered);
            }
            else
            {
                return OrderBooks(param.PageNumber - 1, param.IsDefaultOrdered);
            }
        }

        public BooksIndexModel GetBooksByAuthorAndTitle(BooksIndexModel model)
        {
            return new BooksIndexModel
            {
                Books = SearchBooksByTitleAndAuthor(model.BookSearchData.Title, model.BookSearchData.Author, model.IsDefaultOrdered),
                IsDefaultOrdered = model.IsDefaultOrdered,
                PageNumber = 1,
                BookSearchData = new BookSearchData
                {
                    Title = model.BookSearchData.Title,
                    Author = model.BookSearchData.Author
                }
            };
        }

        // Helper function of GetBooksByTitleAndAuthor method.
        private IEnumerable<Library.Models.Book> SearchBooksByTitleAndAuthor(String title,
            String author, bool isOrderByPopularity)
        {
            if (author == null && title == null)
            {
                return new List<Library.Models.Book>();
            }

            if (title == null)
            {
                return OrderListOfBooks(isOrderByPopularity,
                    _context.Book.Include(b => b.Vols).ThenInclude(z => z.Lendings)
                                    .Where(b => b.Author.Contains(author)).ToList());
            }

            if (author == null)
            {
                return OrderListOfBooks(isOrderByPopularity,
                    _context.Book.Include(b => b.Vols).ThenInclude(z => z.Lendings)
                                    .Where(b => b.Title.Contains(title)).ToList());
            }

            return OrderListOfBooks(isOrderByPopularity,
                _context.Book.Include(b => b.Vols).ThenInclude(z => z.Lendings)
                                .Where(b => b.Title.Contains(title)
                                            && b.Author.Contains(author)).ToList());
        }

        // Helper function: order a list of book by the given ordering type.
        // Needs to make .Include(b => b.Vols).ThenInclude(z => z.Lendings) 
        // on context before calling it.
        private IEnumerable<Library.Models.Book> OrderListOfBooks(
                bool isOrderByPopularity, IEnumerable<Library.Models.Book> books
            )
        {
            if (isOrderByPopularity)
            {
                return books.OrderByDescending(b => b.Vols.SelectMany(v => v.Lendings).Count()).ToList();
            }
            else
            {
                return books.OrderBy(b => b.Title).ToList();
            }
        }

        public Book GetBookByID(int bookID)
        {
            return _context.Book.Include(b => b.Vols)
                                .ThenInclude(v => v.Lendings)
                                .Where(b => b.ID == bookID)
                                .FirstOrDefault();
        }

        #endregion

        #region Database modifier methods

        public LendingViewModel GetCreateLendingView(String serializedLendVolParam)
        {
            LendVolParam lendVolParam = 
                    JsonConvert.DeserializeObject<LendVolParam>(serializedLendVolParam);
            Guest guest = _context.Users.FirstOrDefault(g => g.UserName == lendVolParam.UserName);
            return new LendingViewModel
            {
                VolID = lendVolParam.VolID,
                GuestID = guest.Id
            };
        }

        public Lending CreateNewLending(LendingViewModel lending)
        {
            Lending lend = new Lending
            {
                Vol = _context.Vol.Where(v => v.ID == lending.VolID).FirstOrDefault(),
                Guest = _context.Users.FirstOrDefault(g => g.Id == lending.GuestID),
                StartDay = lending.StartDay,
                EndDay = lending.EndDay
            };
            if (ValidateLending(lend))
            {
                _context.Add(lend);
                _context.SaveChanges();
                return lend;
            }
            else
            {
                return null;
            }
        }

        // Utility function of CreateNewLending method that validating the time period of 
        // the given lending.
        private bool ValidateLending(Lending lending)
        {
            if (lending.StartDay < DateTime.Today)
            {
                return false;
            }
            foreach (var lend in _context.Lending.Where(l => l.Vol.ID == lending.Vol.ID))
            {
                if (lending.StartDay >= lend.StartDay && lending.StartDay <= lend.EndDay
                    || lending.EndDay >= lend.StartDay && lending.EndDay <= lend.EndDay
                    || lending.StartDay <= lend.StartDay && lending.EndDay >= lend.EndDay)
                {
                    return false;
                }
            }
            return true;
        }

        public void DeleteVol(int VolId)
        {
            bool isAvailable = true;
            Vol vol = _context.Vol.Include(v => v.Lendings).ThenInclude(l => l.Vol).Where(v => v.ID == VolId).FirstOrDefault();
            foreach(Lending lend in vol.Lendings)
            {
                if (lend.IsActive == true) isAvailable = false;
                _context.Lending.Remove(lend);
            }
            _context.Vol.Remove(vol);
            if (isAvailable)
            {
                _context.SaveChanges();
            }
            else
            {
                throw new NotAvailableException("Deleteing this volume is not available because it's lended.");
            }
        }


        public void AddBook(Book newBook)
        {
            if(newBook != null)
            {
                _context.Book.Add(newBook);
                _context.SaveChanges();
            }
            else
            {
                throw new NotAvailableException("The book isn't given.");
            }
        }

        public void AddVol(Vol newVol)
        {
            if (newVol != null)
            {
                _context.Vol.Add(newVol);
                _context.SaveChanges();
            }
            else
            {
                throw new NotAvailableException("The volume isn't given.");
            }
        }

        public void ActivateLending(int lendID)
        {
            Lending lendToUpdate = _context.Lending.Where(l => l.ID == lendID).FirstOrDefault();
            lendToUpdate.IsActive = true;
            _context.Lending.Update(lendToUpdate);
            _context.SaveChanges();
        }

        public void InactivateLending(int lendID)
        {
            Lending lendToUpdate = _context.Lending.Where(l => l.ID == lendID).FirstOrDefault();
            lendToUpdate.IsActive = false;
            _context.Lending.Update(lendToUpdate);
            _context.SaveChanges();
        }

        #endregion
    }
}
