using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public interface IGameplayService
    {
        public GameplayViewModel GetGameplayViewModel(String userName, String gameTitle);
        public Field StepGame(String userName, String gameTitle, String direction);
        public Boolean GetIsVisitedField(String userName, String gameTitle, int colNumber, int rowNumber);
    }
}
