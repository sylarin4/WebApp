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
        public User Owner { get; set; }
        public String GameTitle { get; set; }
        public int RowNumber { get; set; }
        public int ColNumber { get; set; }

        // Stores the illustration of the field (can be null).
        // (The user musn't upload image for every field.)
        public Image? Image { get; set; }

        // Describes what happens on this field in the game.
        public String Text { get; set; }


        // If there's trial on this field, stores the ID of it, 
        // if there's not, it's null.
        public Trial? Trial { get; set; }

        // Stores in which directions we can leave from this field.
        public Boolean IsRightWay { get; set; }
        public Boolean IsLeftWay { get; set; }
        public Boolean IsUpWay { get; set; }
        public Boolean IsDownWay { get; set; }
    }
}
