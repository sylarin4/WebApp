﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class CreateMapContentViewModel
    {
        public String GameTitle { get; set; }
        public List<MapRow> Map { get; set; }
        public int MapSize { get; set; }
        public String MapPieceText { get; set; }
    }
}
