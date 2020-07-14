using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models
{
    public class Vol
    {
        public int ID { get; set; }
        public Book Book { get; set; }
        public virtual ICollection<Lending> Lendings { get; set; }
    }
}
