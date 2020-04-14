using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class Prelude
    {
        public int ID { get; set; }
        public String Text { get; set; }
        public String GameTitle { get; set; }
        public User Owner { get; set; }
        public byte[] Image { get; set; }
    }
}
