using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Newtonsoft.Json;
using Library.Contexts;
using Library.Data;

namespace Library.Models
{
    [Route("api/Librarian")]
    public class AdminController : ControllerBase
    {
        private readonly LibraryContext _libraryContext;
        private readonly LibraryService _libraryService;

        public AdminController(LibraryContext libraryContext)
        {
            _libraryContext = libraryContext;
            _libraryService = new LibraryService(_libraryContext);
        }
               

        #region Test post and get

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Hello World!");
        }
        
        [HttpGet]
        [Route("Test")]
        public IActionResult Test()
        {
            Trace.WriteLine("Success");
            return Ok();
        }

        [HttpPost]
        [Route("TestPost")]
        public async Task<IActionResult> TestPost([FromBody]UserDTO data)
        {
            Trace.WriteLine("Testpost Succeed " + data.UserName);
            return Ok();
        }

        #endregion

        #region List books and lendings
        
        [HttpGet]
        [Route("ListBooks")]
        public IActionResult ListBooks()
        {
            List<BookDTO> result = new List<BookDTO>();
            foreach(var book in _libraryService.ListBooks())
            {
                result.Add(new BookDTO()
                {
                    ID = book.ID,
                    ISBN = book.ISBN,
                    Author = book.Author,
                    Title = book.Title,
                    ReleaseYear = book.ReleaseYear,
                    VolIDs = _libraryService.GetVolsByBookId(book.ID)
                });
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("ListLendings")]
        public IActionResult ListLendings()
        {
            List<LendingDTO> result = new List<LendingDTO>();
            foreach(var lending in _libraryService.ListLendings())
            {
                result.Add(new LendingDTO()
                {
                    ID = lending.ID,
                    VolID = lending.Vol.ID,
                    GuestID = lending.Guest.Id,
                    StartDay = lending.StartDay,
                    EndDay = lending.EndDay,
                    IsActive = lending.IsActive
                });
            }
            return Ok(result);
        }
        #endregion

        #region Database modifier methods

        [HttpPost]
        [Route("DeleteVol")]
        public IActionResult DeleteVol([FromBody] int volId)
        {
            try
            {
                _libraryService.DeleteVol(volId);
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("AddBook")]
        public IActionResult AddBook([FromBody] BookDTO bookDTO)
        {
            try
            {
                _libraryService.AddBook(new Book
                {
                    Title = bookDTO.Title,
                    Author = bookDTO.Author,
                    ReleaseYear = bookDTO.ReleaseYear,
                    ISBN = bookDTO.ISBN
                });
                return Ok();
            }
            catch(Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("AddVol")]
        public IActionResult AddVol([FromBody] int bookID)
        {
            Book book = _libraryContext.Book.Where(b => b.ID == bookID).FirstOrDefault();
            if (book != null)
            {
                try
                {
                    _libraryService.AddVol(new Vol
                    {
                        Book = book
                    });
                    return Ok();
                }
                catch
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("ActivateLending")]
        public IActionResult ActivateLending([FromBody] int lendID)
        {
            try
            {
                _libraryService.ActivateLending(lendID);
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("InactivateLending")]
        public IActionResult InactivateLending([FromBody] int lendID)
        {
            try
            {
                _libraryService.InactivateLending(lendID);
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion
    }
}
