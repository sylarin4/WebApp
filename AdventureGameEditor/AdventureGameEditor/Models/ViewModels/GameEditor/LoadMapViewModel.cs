using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models.ViewModels.GameEditor
{
    public class LoadMapViewModel
    {
        public int MapSize { get; set; }
        public String GameTitle { get; set; }
        public List<MapRow> Map { get; set; }

        // Functon's name to call when click on a button of the map. (Js. script function.)
        public String FunctionName { get; set; }
    }
}
