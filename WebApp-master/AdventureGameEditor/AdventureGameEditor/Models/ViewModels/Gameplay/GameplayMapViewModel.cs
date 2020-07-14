using System;
using System.Collections.Generic;


namespace AdventureGameEditor.Models.ViewModels.Gameplay
{
    public class GameplayMapViewModel
    {
        public String GameTitle { get; set; }
        public List<List<GameplayFieldViewModel>> GameplayMap { get; set; }
    }
}
