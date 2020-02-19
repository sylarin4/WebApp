using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    // Stores in which directions you can leave a map's field.
    public class ExitRoads
    {
        public int ID { get; set; }
        public Boolean IsRightWay { get; set; }
        public Boolean IsLeftWay { get; set; }
        public Boolean IsUpWay { get; set; }
        public Boolean IsDownWay { get; set; }
    }
}
