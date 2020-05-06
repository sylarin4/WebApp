using System;
using System.Collections.Generic;

using AdventureGameEditor.Models.Enums;

namespace AdventureGameEditor.Models
{
    public class GameplayData
    {
        public int ID { get; set; }
        public String PlayerName { get; set; }
        public String GameTitle { get; set; }
        public Field CurrentPlayerPosition { get; set; }
        public int StepCount { get; set; }
        public GameCondition GameCondition { get; set; }
        public ICollection<IsVisitedField> VisitedFields { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime LastPlayDate { get; set; }
        public int LifeCount { get; set; }
    }
    
}
