using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models.DatabaseModels.Gameplay
{
    public class IsVisitedField
    {
        public int ID { get; set; }
        public Boolean IsVisited { get; set; }
        public int ColNumber { get; set; }
        public int RowNumber { get; set; }
    }
}
