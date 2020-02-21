

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class MapRow
    {
        public int ID { get; set; }
        public virtual ICollection<Field> Row { get; set; }
    }
}
