using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    // Stores the data of a field of a map.
    public class Field
    {
        public int ID { get; set; }
        //public FieldCoordinate FieldCoordinate { get; set; }
        public int HorizontalCord { get; set; }
        public int VerticalCord { get; set; }

        //Describes what happens on this field in the game.
        public String Text { get; set; }


        // If there's trial on this field, stores the ID of it, 
        // if there's not, it's null.
        public int? TrialID { get; set; }

        // Stores in which directions we can leave from this field.
        public Boolean IsRightWay { get; set; }
        public Boolean IsLeftWay { get; set; }
        public Boolean IsUpWay { get; set; }
        public Boolean IsDownWay { get; set; }
    }
}
