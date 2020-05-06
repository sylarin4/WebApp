using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models.ViewModels.GameEditor
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

        // Short guide about how to do the current editing method by the program.
        public String Guide { get; set; }

        // The next creation phase's controller action.
        // (The function's name which will be called from the GameEditorController after user 
        // clicked on the "Tovább" (next) link.)
        public String NextControllerAction { get; set; }

        // If we reload the page after adding text or trial to the field, we reload the buttons for the selected field (thanks to 
        // tehese informations), so the user don't need to select again the edited field to add more content or show it or edit it.
        public Boolean IsFieldSelected { get; set; }
        public Field SelectedField { get; set; }
        public String ErrorMessage { get; set; }
    }
}
