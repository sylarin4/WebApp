using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class Message
    {
        public int ID { get; set; }
        //public User Owner { get; set; } -- is this really required?
        public String Text { get; set; }
        public DateTime Date { get; set; }
    }
}
