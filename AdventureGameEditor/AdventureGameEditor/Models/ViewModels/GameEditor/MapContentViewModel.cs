using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class MapContentViewModel
    {
        public String GameTitle { get; set; }
        public List<MapRow> Map { get; set; }
        public int MapSize { get; set; }

        // The function's name which should be called when user clicks on a field of the map. (From site.js)
        public String FunctionName { get; set; }

        // The description of the creation phase to view it on the screen, so the user will know what
        // can he/she make there.
        public String Action { get; set; }

        // The next creation phase's controller action.
        // (The function's name which will be called from the GameEditorController after user 
        // clicked on the "Tovább" (next) link.)
        public String NextControllerAction { get; set; }
    }
}
