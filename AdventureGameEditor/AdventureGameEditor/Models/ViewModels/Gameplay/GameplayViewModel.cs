using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class GameplayViewModel
    {
        public User Player { get; set; }
        public String GameTitle { get; set; }
        public Field CurrentPlayerPosition { get; set; }
        public Field TargetField { get; set; }
        public int StepCount { get; set; }
        public Boolean IsGameOver { get; set; }
        public int LifeCount { get; set; }
        public Boolean IsVisitiedCurrentPlayerPosition { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime LastPlayDate { get; set; }
        //public List<List<GameplayFieldViewModel>> GameplayMap { get; set; }
    }
}
