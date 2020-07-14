using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Library.Models
{
    public interface ILibraryService
    {

        // Order books by popularity or title.
        BooksIndexModel OrderBooks(int pageNumber, bool isOrderByPopularity);
            
        // Get the books of the next or the previous page if it's exists.
        BooksIndexModel GetPageBooks(String serializedParam, bool isNextPage);

        // Returns a BooksIndexModel containing the searched books.
        BooksIndexModel GetBooksByAuthorAndTitle(BooksIndexModel model);

        // Returns the book with the given ID.
        Book GetBookByID(int bookID);

        LendingViewModel GetCreateLendingView(String serializedLendVolParam);

        Lending CreateNewLending(LendingViewModel lending);

        // Deletes the volume with the given Id if it isn't lended.
        void DeleteVol(int VolId);

        void AddBook(Book newBook);

        void AddVol(Vol newVol);

        void ActivateLending(int lendID);

        void InactivateLending(int lendID);
    }
}
