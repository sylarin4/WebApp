using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models
{
    // Model of Books' Index view. Containg the information needed by the Index view.
    public class BooksIndexModel
    {
        public IEnumerable<Library.Models.Book> Books { get; set; }
        public bool IsDefaultOrdered { get; set; }
        public int PageNumber { get; set; }
        public BookSearchData BookSearchData { get; set; }

    }
}
