using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class DirectionButtonsViewModel
    {
        public String GameTitle { get; set; }
        public int RowNumber { get; set; }
        public int ColNumber { get; set; }
        public Boolean GameLost { get; set; }
        public Boolean GameWon { get; set; }
        public Boolean IsUpWay { get; set; }
        public Boolean IsDownWay { get; set; }
        public Boolean IsRightWay { get; set; }
        public Boolean IsLeftWay { get; set; }
    }
}
