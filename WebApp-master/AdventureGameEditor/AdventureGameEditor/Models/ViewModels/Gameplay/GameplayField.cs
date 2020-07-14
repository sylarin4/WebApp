using System;

using AdventureGameEditor.Models.DatabaseModels.Game;

namespace AdventureGameEditor.Models.ViewModels.Gameplay
{
    public class GameplayFieldViewModel
    {
        public String GameTitle { get; set; }
        public int RowNumber { get; set; }
        public int ColNumber { get; set; }
        public String Text { get; set; }
        public int FieldImageID { get; set; }
        public Trial? Trial { get; set; }
        public Boolean IsRightWay { get; set; }
        public Boolean IsLeftWay { get; set; }
        public Boolean IsUpWay { get; set; }
        public Boolean IsDownWay { get; set; }
        public Boolean IsVisited { get; set; }
        public Boolean IsAtTargetField { get; set; }
        public Boolean IsGameOver { get; set; }
        public int LifeCount { get; set; }
    }
}
