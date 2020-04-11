using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.IO;

using AdventureGameEditor.Data;

namespace AdventureGameEditor.Models
{
    public class GameplayService : IGameplayService
    {
        protected readonly AdventureGameEditorContext _context;

        #region Constructor
        public GameplayService(AdventureGameEditorContext context)
        {
            _context = context;
        }

        #endregion

        #region Initialize Gameplay
        public GameplayViewModel GetGameplayViewModel(String userName, String gameTitle)
        {
            GameplayData gameplayData = _context.GameplayData
                .Where(gameplay => gameplay.Player.UserName == userName && gameplay.GameTitle == gameTitle)
                .Include(gameplay => gameplay.VisitedFields)
                .Include(data => data.CurrentPlayerPosition)
                .ThenInclude(field => field.Trial)
                .ThenInclude(trial => trial.Alternatives)
                .FirstOrDefault();

            Game game = GetGame(userName, gameTitle);
            // Check if we have to initialize.
            if (gameplayData == null)
            {
                gameplayData = new GameplayData()
                {
                    Player = _context.User.Where(user => user.UserName == userName).FirstOrDefault(),
                    GameTitle = gameTitle,
                    CurrentPlayerPosition = game.StartField,
                    StepCount = 0,
                    GameCondition = GameCondition.OnGoing,
                    VisitedFields = InitializeVisitedFields(game.TableSize),
                    StartDate = DateTime.Now,
                    LastPlayDate = DateTime.Now
                };
                _context.GameplayData.Add(gameplayData);
            }
            _context.SaveChanges();

            // Initialize GameplayViewModel

            GameplayViewModel gameplayViewModel = new GameplayViewModel()
            {
                Player = _context.User.Where(user => user.UserName == userName).FirstOrDefault(),
                GameTitle = gameTitle,
                CurrentPlayerPosition = gameplayData.CurrentPlayerPosition,
                TargetField = game.TargetField,
                StepCount = gameplayData.StepCount,
                IsGameOver = gameplayData.GameCondition != GameCondition.OnGoing,
                GameplayMap = InitializeGameplayMap(game.Map.ToList()),
                StartDate = gameplayData.StartDate,
                LastPlayDate = DateTime.Now,
                IsVisitiedCurrentlayerPosition = gameplayData.VisitedFields
                    .Where(field => field.RowNumber == gameplayData.CurrentPlayerPosition.RowNumber
                    && field.ColNumber == gameplayData.CurrentPlayerPosition.ColNumber).FirstOrDefault().IsVisited
            };

            return gameplayViewModel;
        }

        public List<List<GameplayFieldViewModel>> InitializeGameplayMap(List<MapRow> map)
        {
            List<List<GameplayFieldViewModel>> gameplayMap = new List<List<GameplayFieldViewModel>>();
            foreach (MapRow row in map)
            {
                List<GameplayFieldViewModel> gameplayRow = new List<GameplayFieldViewModel>();
                foreach (Field field in row.Row)
                {
                    gameplayRow.Add(new GameplayFieldViewModel()
                    {
                        RowNumber = field.RowNumber,
                        ColNumber = field.ColNumber,
                        Text = field.Text,
                        Trial = field.Trial,
                        IsRightWay = field.IsRightWay,
                        IsLeftWay = field.IsLeftWay,
                        IsUpWay = field.IsUpWay,
                        IsDownWay = field.IsDownWay,
                        IsVisited = false
                    });
                }
                gameplayMap.Add(gameplayRow);
            }
            gameplayMap = OrderGameplayMap(gameplayMap);
            return gameplayMap;
        }

        public List<IsVisitedField> InitializeVisitedFields(int mapSize)
        {
            List<IsVisitedField> visitedFields = new List<IsVisitedField>();
            for (int i = 0; i < mapSize; ++i)
            {
                for (int j = 0; j < mapSize; ++j)
                {
                    visitedFields.Add(new IsVisitedField()
                    {
                        IsVisited = false,
                        ColNumber = i,
                        RowNumber = j
                    });
                }
            }
            return visitedFields;
        }

        #endregion

        #region Step and load game

        public Field StepGame(String userName, String gameTitle, String direction)
        {
            GameplayData gameplayData = GetGameplayData(userName, gameTitle);

            // Set the last field to visited.
            gameplayData.VisitedFields
                .Where(field => field.ColNumber == gameplayData.CurrentPlayerPosition.ColNumber
                    && field.RowNumber == gameplayData.CurrentPlayerPosition.RowNumber)
                .FirstOrDefault()
                .IsVisited = true;

            Field playerPosition = gameplayData.CurrentPlayerPosition;
            int newColNumber = playerPosition.ColNumber;
            int newRowNumber = playerPosition.RowNumber;
            int tableSize = GetGame(userName, gameTitle).TableSize;
            switch (direction)
            {
                case "Up":
                    if (playerPosition.IsUpWay && playerPosition.RowNumber > 0)
                        newRowNumber = playerPosition.RowNumber - 1;
                    break;
                case "Right":
                    if (playerPosition.IsRightWay && playerPosition.ColNumber + 1 < tableSize)
                        newColNumber = playerPosition.ColNumber + 1;
                    break;
                case "Down":
                    if (playerPosition.IsDownWay && playerPosition.RowNumber + 1 < tableSize)
                        newRowNumber = playerPosition.RowNumber + 1;
                    break;
                case "Left":
                    if (playerPosition.IsLeftWay && playerPosition.ColNumber > 0)
                        newColNumber = playerPosition.ColNumber - 1;
                    break;
            }
            gameplayData.CurrentPlayerPosition = GetField(userName, gameTitle, newColNumber, newRowNumber);
            gameplayData.StepCount++;
            gameplayData.LastPlayDate = DateTime.Now;
            gameplayData.GameCondition =
                (IsAtTargetField(gameTitle, newRowNumber, newColNumber) && GetTrial(gameTitle, newColNumber, newRowNumber) == null)
                ? GameCondition.Won : GameCondition.OnGoing;
            Trace.WriteLine(gameplayData.ID + " " +
                gameplayData.Player.UserName + " " +
                gameplayData.GameTitle + " " +
                gameplayData.CurrentPlayerPosition.ColNumber + " " +
                gameplayData.CurrentPlayerPosition.RowNumber + " " +
                gameplayData.StepCount + " " +
                gameplayData.GameCondition + " " +
                gameplayData.VisitedFields.Count + " " +
                gameplayData.StartDate + " " +
                gameplayData.LastPlayDate);
            _context.SaveChanges();
            return gameplayData.CurrentPlayerPosition;
        }

        public GameResult GetGameResult(String gameTitle, String userName)
        {
            Boolean isgameWon = GetGameplayData(userName, gameTitle).GameCondition == GameCondition.Won;
            DeleteGameplayData(userName, gameTitle);
            return _context.GameResult.Where(result => result.GameTitle == gameTitle && result.IsGameWonResult == isgameWon)
                .FirstOrDefault();
        }

        #endregion

        #region Usually used getters

        public Boolean GetIsVisitedField(String userName, String gameTitle, int colNumber, int rowNumber)
        {
            return GetGameplayData(userName, gameTitle).VisitedFields
                .Where(field => field.ColNumber == colNumber && field.RowNumber == rowNumber)
                .Select(field => field.IsVisited)
                .FirstOrDefault();
        }
        public Trial GetTrial(String gameTitle, int colNumber, int rowNumber)
        {
            return _context.Field
                .Where(field => field.GameTitle == gameTitle && field.ColNumber == colNumber && field.RowNumber == rowNumber)
                .Include(field => field.Trial)
                .ThenInclude(trial => trial.Alternatives)
                .ThenInclude(alt => alt.TrialResult)
                .Select(field => field.Trial)
                .FirstOrDefault();
        }
        public Boolean IsAtTargetField(String gameTitle, int rowNumber, int colNumber)
        {
            return _context.Game
                .Where(game => game.TargetField.ColNumber == colNumber && game.TargetField.RowNumber == rowNumber)
                .ToList().Count > 0;
        }

        public Field GetField(String gameTitle, int rowNumber, int colNumber)
        {
            return _context.Field
                .Where(field => field.GameTitle == gameTitle && field.ColNumber == colNumber && field.RowNumber == rowNumber)
                .Include(field => field.Trial)
                .ThenInclude(trial => trial.Alternatives)
                .ThenInclude(alternative => alternative.TrialResult)
                .FirstOrDefault();
        }
        #endregion

        #region Helper functions



        private Game GetGame(String userName, String gameTitle)
        {
            return _context.Game
                .Where(game => game.Owner.UserName == userName && game.Title == gameTitle)
                .Include(game => game.Map)
                .ThenInclude(map => map.Row)
                .ThenInclude(field => field.Trial)
                .ThenInclude(trial => trial.Alternatives)
                .FirstOrDefault();
        }

        private GameplayData GetGameplayData(String userName, String gameTitle)
        {
            return _context.GameplayData
                .Include(data => data.Player)
                .Include(data => data.CurrentPlayerPosition)
                .Where(data => data.Player.UserName == userName && data.GameTitle == gameTitle)
                .Include(data => data.VisitedFields)
                .FirstOrDefault();
        }

        private Field GetField(String userName, String gameTitle, int colNumber, int rowNumber)
        {
            return _context.Field.Where(field => field.Owner.UserName == userName && field.GameTitle == gameTitle
            && field.ColNumber == colNumber && field.RowNumber == rowNumber)
                .Include(field => field.Trial)
                .ThenInclude(trial => trial.Alternatives)
                .FirstOrDefault();
        }

        private List<List<GameplayFieldViewModel>> OrderGameplayMap(List<List<GameplayFieldViewModel>> map)
        {
            for (int i = 0; i < map.Count; ++i)
            {
                map[i] = map[i].OrderBy(row => row.ColNumber).ToList();
            }
            return map;
        }

        private void DeleteGameplayData(String playerName, String gameTitle)
        {
            GameplayData dataToDelete = _context.GameplayData.Where(data => data.Player.UserName == playerName && data.GameTitle == gameTitle
            ).FirstOrDefault();
            if(dataToDelete != null)
            {
            _context.GameplayData.Remove(dataToDelete);
            }
            _context.SaveChanges();
        }



        #endregion
    }
}
