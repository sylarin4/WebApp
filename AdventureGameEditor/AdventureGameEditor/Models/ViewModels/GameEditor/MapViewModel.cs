using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models.ViewModels.GameEditor
{
    public class MapViewModel
    {
        public int MapSize { get; set; }
        public String GameTitle { get; set; }
        public List<MapRow> Map { get; set; }       
            
    }
}
