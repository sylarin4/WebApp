using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    // For storeing a game field's map coordinates.
    public class FieldCoordinate
    {
        public int ID { get; set; }
        public int HorizontalCoordinate { get; set; }
        public int VerticalCoordinate { get; set; }
    }
}
