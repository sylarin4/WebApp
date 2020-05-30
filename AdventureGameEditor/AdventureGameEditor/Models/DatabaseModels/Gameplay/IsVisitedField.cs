using System;

namespace AdventureGameEditor.Models.DatabaseModels.Gameplay
{
    public class IsVisitedField
    {
        public int ID { get; set; }
        public Boolean IsVisited { get; set; }
        public int ColNumber { get; set; }
        public int RowNumber { get; set; }
        public GameplayData GameplayData { get; set; }
    }
}
