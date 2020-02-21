using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class MapViewModel
    {

        public int TargetPicID { get; set; }
        public int TargetCellID { get; set; }
        public int TargetRowID { get; set; }
        public int MapSize { get; set; }
        public String GameTitle { get; set; }
        public List<MapRow> Map { get; set; }       
            
    }
}
