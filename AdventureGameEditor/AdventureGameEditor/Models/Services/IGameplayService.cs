using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using AdventureGameEditor.Models.ViewModels.Gameplay;
using AdventureGameEditor.Models.Enums;

namespace AdventureGameEditor.Models.Services
{
    public interface IGameplayService
    {
        public GameplayViewModel GetGameplayViewModel(String userName, String gameTitle);
        public void StepPlayCounter(String gameTitle);
        public Field StepGame(String userName, String gameTitle, Direction direction);
        public Boolean GetIsVisitedField(String userName, String gameTitle, int colNumber, int rowNumber);
        public Trial GetTrial(String gameTitle, int colNumber, int rowNumber);
        public Boolean IsAtTargetField(String gameTitle, int rowNumber, int colNumber);
        public Field GetField(String gameTitle, int rowNumber, int colNumber);
        public GameResult GetGameResult(String gameTitle, String userName);
        public FileContentResult GetFieldImage(int imageID);
        public void SetGameOver(String playerName, String gameTitle, Boolean isGameWon);
        public DirectionButtonsViewModel GetDirectionButtonsAfterTrial(String playerName, String gameTitle, int colNumber,
            int rowNumber, int trialNumber, Boolean isAtTargetField);
        public int GetLifeCount(String playerName, String gameTitle);
        public Direction GetRandDirection();
        public Field GetNextField(Field currentField, int tableSize, Direction direction);
        public int GetGameMapSize(String gameTitle);
        public GameplayFieldViewModel GetGameplayFieldViewModel(String playerName, String gameTitle, Field field);
        public CompassPoint GetTargetDirection(String gameTitle, Field field);
    }
}
