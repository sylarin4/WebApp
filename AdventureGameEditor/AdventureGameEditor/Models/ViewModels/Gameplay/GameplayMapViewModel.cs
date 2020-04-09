using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class GameplayMapViewModel
    {
        public String GameTitle { get; set; }
        public List<List<GameplayFieldViewModel>> GameplayMap { get; set; }
    }
}
