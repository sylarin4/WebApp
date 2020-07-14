using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using Library.Models;
using Library.Contexts;

namespace Library.Controllers
{
    public class BooksController : BaseController
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context, ILibraryService libraryService) 
            : base(libraryService)
        {
            _context = context;
        }

        // GET: Books
        public  IActionResult Index()
        {
            return ListBooksByPopularity(1);
        }
        
        #region Automatically generated code
        
        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .SingleOrDefaultAsync(m => m.ID == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,Author,ReleaseYear,ISBN")] Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.SingleOrDefaultAsync(m => m.ID == id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,Author,ReleaseYear,ISBN")] Book book)
        {
            if (id != book.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .SingleOrDefaultAsync(m => m.ID == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Book.SingleOrDefaultAsync(m => m.ID == id);
            _context.Book.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.ID == id);
        }

        #endregion

        #region Listing books

        public IActionResult ListBooksByPopularity(int pageNumber)
        {
            return View("Index", _libraryService.OrderBooks(pageNumber, true));
        }
       
        public IActionResult ListBooksByTitle(int pageNumber)
        {
            return View("Index", _libraryService.OrderBooks(pageNumber, false));
        }

        #endregion

        #region Get next and previous page

        public IActionResult GetNextPage(String parameter)
        {
            return View("Index", _libraryService.GetPageBooks(parameter, true));
        }

        public IActionResult GetPreviousPage(String parameter)
        {
            return View("Index", _libraryService.GetPageBooks(parameter, false));
        }

        #endregion

        #region Load image
        public FileResult ImageForBook(Int32? bookISBN)
        {

            if (bookISBN == null)
                return File("~/images/not_found.png", "image/png");

            Byte[] imageContent = _context.BookImage
                .Where(image => image.BookISBN == bookISBN)
                .Select(image => image.Image)
                .FirstOrDefault();

            if (imageContent == null)
                return File("~/images/not_found.png", "image/png");

            return File(imageContent, "image/png");
        }

        #endregion

        #region Search by Author and Title

        //[HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SearchByAuthorAndTitle(Library.Models.BooksIndexModel model)
        {
            return View("Index", _libraryService.GetBooksByAuthorAndTitle(model));
        }

        #endregion

        #region Show book detail

        public IActionResult ShowBookDetail(int bookID)
        {
            return View("Details", _libraryService.GetBookByID(bookID));
        }

        #endregion
    }
}
