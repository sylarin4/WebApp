using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.Data;
using Library.Models;
using Library.Contexts;

namespace Library.Models
{
    public class Converters
    {

        public BookDTO BookToBookDTO(Book book)
        {
            return new BookDTO()
            {
                ID = book.ID,
                ISBN = book.ISBN,
                Author = book.Author,
                Title = book.Title,
                ReleaseYear = book.ReleaseYear,
                VolIDs = VolsToVolIDs(book.Vols)
            };
        }

        public List<int> VolsToVolIDs(IEnumerable<Vol> vols)
        {
            List<int> volIDs = new List<int>();
            foreach(Vol vol in vols)
            {
                volIDs.Add(vol.ID);
            }
            return volIDs;
        }

        public static Book BookDTOToBook(BookDTO bookDTO)
        {
            Book result = new Book()
            {
                ID = bookDTO.ID,
                Title = bookDTO.Title,
                Author = bookDTO.Author,
                ReleaseYear = bookDTO.ReleaseYear,
                ISBN = bookDTO.ISBN
            };
            List<Vol> vols = new List<Vol>();
            foreach(int volID in bookDTO.VolIDs)
            {
                vols.Add(new Vol()
                {
                    ID = volID,
                    Book = result,
                    //VolID = volID
                });
            }
            result.Vols = vols;
            return result;
        }
        
    }
}
