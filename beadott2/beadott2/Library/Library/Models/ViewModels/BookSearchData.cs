using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models
{
    public class BookSearchData
    {
        public BookSearchData()
        {
            this.Title = "";
            this.Author = "";
        }
        public String Title { get; set; }
        public String Author { get; set; }
    }
}
