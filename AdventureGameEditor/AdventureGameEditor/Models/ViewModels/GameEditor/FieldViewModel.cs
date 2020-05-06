using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models.ViewModels.GameEditor
{
    public class FieldViewModel
    {
        public int HorizontalCord { get; set; }
        public int VerticalCord { get; set; }

        //Describes what happens on this field in the game.
        public String Text { get; set; }


        // If there's trial on this field, stores the ID of it, 
        // if there's not, it's null.
        public int? TrialID { get; set; }

        // Stores in which directions can we leave from this field.
        public WayDirectionsViewModel WayDirection { get; set; }
    }
}
