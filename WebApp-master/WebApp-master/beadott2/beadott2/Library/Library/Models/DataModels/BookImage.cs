using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models
{
    public class BookImage
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
        public int BookISBN { get; set; }
    }
}

