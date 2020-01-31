using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models
{
    public class Book
    {
        public int ID { get; set; }
        public String Title { get; set; }
        public String Author { get; set; }
        public int ReleaseYear { get; set; }
        public int ISBN { get; set; }
        public virtual ICollection<Vol> Vols { get; set; }
    }
}

