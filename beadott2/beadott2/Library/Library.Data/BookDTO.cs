using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Data
{
    public class BookDTO
    {
        public int ID { get; set; }
        public int ISBN { get; set; }
        public String Author { get; set; }
        public String Title { get; set; }
        public int ReleaseYear { get; set; }
        public List<int> VolIDs { get; set; }
        public String VolIDsToString { get
            {
                String result = "";
                foreach (var volId in this.VolIDs)
                {
                    result += volId + ", ";
                }
                return result;
            }
        }
    }
}
