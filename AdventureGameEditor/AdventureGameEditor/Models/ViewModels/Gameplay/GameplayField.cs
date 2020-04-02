﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class GameplayField
    {
        public int RowNumber { get; set; }
        public int ColNumber { get; set; }
        public String Text { get; set; }
        public Trial? Trial { get; set; }
        public Boolean IsRightWay { get; set; }
        public Boolean IsLeftWay { get; set; }
        public Boolean IsUpWay { get; set; }
        public Boolean IsDownWay { get; set; }
        public Boolean IsVisited { get; set; }
    }
}
