using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.IO;

using AdventureGameEditor.Data;
using Org.BouncyCastle.Asn1.X509;
using AdventureGameEditor.Migrations;
using Org.BouncyCastle.Asn1.Mozilla;

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
            if(userName == null)
            {
                return null;
            }
            GameplayData gameplayData = _context.GameplayData
                .Where(gameplay => gameplay.PlayerName == userName && gameplay.GameTitle == gameTitle)
                .Include(gameplay => gameplay.VisitedFields)
                .Include(data => data.CurrentPlayerPosition)
                .ThenInclude(field => field.Trial)
                .ThenInclude(trial => trial.Alternatives)
                .FirstOrDefault();

            Game game = GetGame(gameTitle);

            // Check if we have to initialize.
            if (gameplayData == null)
            {
                int lifeCount = 3;
                if (game.TableSize > 5) lifeCount = 5;
                if (game.TableSize > 10) lifeCount = 7;
                gameplayData = new GameplayData()
                {
                    PlayerName = userName,
                    GameTitle = gameTitle,
                    CurrentPlayerPosition = game.StartField,
                    StepCount = 0,
                    GameCondition = GameCondition.OnGoing,
                    VisitedFields = InitializeVisitedFields(game.TableSize),
                    StartDate = DateTime.Now,
                    LastPlayDate = DateTime.Now,
                    LifeCount = lifeCount
                };
                _context.GameplayData.Add(gameplayData);
            }
            _context.SaveChanges();

            // Initialize GameplayViewModel

            GameplayViewModel gameplayViewModel = new GameplayViewModel()
            {
                Player = _context.User.Where(user => user.UserName == userName).FirstOrDefault(),
                GameTitle = gameTitle,
                CurrentPlayerPosition = GetField(gameTitle, gameplayData.CurrentPlayerPosition.RowNumber, gameplayData.CurrentPlayerPosition.ColNumber),
                TargetField = game.TargetField,
                StepCount = gameplayData.StepCount,
                IsGameOver = gameplayData.GameCondition != GameCondition.OnGoing,
                GameplayMap = InitializeGameplayMap(game.Map.ToList()),
                StartDate = gameplayData.StartDate,
                LastPlayDate = DateTime.Now,
                LifeCount = gameplayData.LifeCount,
                IsVisitiedCurrentPlayerPosition = gameplayData.VisitedFields
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

        public void StepPlayCounter(String gameTitle)
        {
            GetGame(gameTitle).PlayCounter++;
            _context.SaveChanges();
        }

        public Field StepGame(String userName, String gameTitle, String direction)
        {
            GameplayData gameplayData = GetGameplayData(userName, gameTitle);
            // Set the last field to visited.
            gameplayData.VisitedFields
                .Where(field => field.ColNumber == gameplayData.CurrentPlayerPosition.ColNumber
                    && field.RowNumber == gameplayData.CurrentPlayerPosition.RowNumber)
                .FirstOrDefault()
                .IsVisited = true;

            // Step the player to the next field.
            Field playerPosition = gameplayData.CurrentPlayerPosition;
            int newColNumber = playerPosition.ColNumber;
            int newRowNumber = playerPosition.RowNumber;
            int tableSize = GetGame(gameTitle).TableSize;
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
            gameplayData.CurrentPlayerPosition = GetField(gameTitle, newRowNumber, newColNumber);
            gameplayData.StepCount++;
            gameplayData.LastPlayDate = DateTime.Now;
            // GameCondition just changes here when we are at the target field.
            gameplayData.GameCondition =
                (IsAtTargetField(gameTitle, newRowNumber, newColNumber) && GetTrial(gameTitle, newColNumber, newRowNumber) == null)
                ? GameCondition.Won : GameCondition.OnGoing;
          
            _context.SaveChanges();
            return gameplayData.CurrentPlayerPosition;
        }

        public GameResult GetGameResult(String gameTitle, String userName)
        {
            Boolean isgameWon = GetGameplayData(userName, gameTitle).GameCondition == GameCondition.Won;
            DeleteGameplayData(userName, gameTitle);
            return _context.GameResult
                .Where(result => result.GameTitle == gameTitle && result.IsGameWonResult == isgameWon)
                .Include(result => result.Image)
                .FirstOrDefault();
        }

        public void SetGameOver(String playerName, String gameTitle, Boolean isGameWon)
        {
            GameplayData gameplayData = GetGameplayData(playerName, gameTitle);
            if (isGameWon)
            {
                gameplayData.GameCondition = GameCondition.Won;
            }
            else
            {
                gameplayData.GameCondition = GameCondition.Lost;
            }
            _context.SaveChanges();
        }

        #endregion

        public DirectionButtonsViewModel GetDirectionButtonsAfterTrial(String playerName, String gameTitle, int colNumber,
            int rowNumber, int trialNumber, Boolean isAtTargetField)
        {
            Field field = GetField(gameTitle, rowNumber, colNumber);
            GameplayData gameplayData = GetGameplayData(playerName, gameTitle);
            DirectionButtonsViewModel model = new DirectionButtonsViewModel()
            {
                GameTitle = gameTitle,
                RowNumber = rowNumber,
                ColNumber = colNumber,
                GameLost = false,
                GameWon = false,
                IsDownWay = field.IsDownWay,
                IsLeftWay = field.IsLeftWay,
                IsRightWay = field.IsRightWay,
                IsUpWay = field.IsUpWay
            };
            switch (GetTrial(gameTitle, colNumber, rowNumber).Alternatives[trialNumber].TrialResult.ResultType)
            {
                case ResultType.GameLost:
                    gameplayData.LifeCount--;

                    // If this was the last life, game is lost.
                    if (gameplayData.LifeCount < 1)
                    {
                        // Set game lost.
                        SetGameOver(playerName, gameTitle, false);
                        model.GameLost = true;
                    }
                    _context.SaveChanges();
                    return model;
                case ResultType.GameWon:
                    // Set game won.
                    SetGameOver(playerName, gameTitle, true);
                    model.GameWon = true;
                    return model;
                default:
                    if (isAtTargetField)
                    {
                        // Set game won.
                        SetGameOver(playerName, gameTitle, true);
                        model.GameWon = true;
                    }
                    return model;
            }
        }

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
           Field targetField = _context.Game
                                       .Where(game => game.Title == gameTitle)
                                       .Select(game => game.TargetField)
                                       .FirstOrDefault();
            return (targetField.ColNumber == colNumber && targetField.RowNumber == rowNumber) ;
        }

        public Field GetField(String gameTitle, int rowNumber, int colNumber)
        {
            return _context.Field
                .Where(field => field.GameTitle == gameTitle && field.ColNumber == colNumber && field.RowNumber == rowNumber)
                .Include(field => field.Image)
                .Include(field => field.Trial)
                .ThenInclude(trial => trial.Alternatives)
                .ThenInclude(alternative => alternative.TrialResult)
                .FirstOrDefault();
        }

        public FileContentResult GetFieldImage(int imageID)
        {
            if (imageID < 0) return null;
            byte[] picture = _context.Field.Where(field => field.Image.ID == imageID).Select(field => field.Image.Picture).FirstOrDefault();
            if (picture == null) return null;
            return new FileContentResult(picture, "image/png");
        }

        public int GetLifeCount(String playerName, String gameTitle)
        {
            return _context.GameplayData.Where(data => data.PlayerName == playerName && data.GameTitle == gameTitle)
                                        .Select(data => data.LifeCount)
                                        .FirstOrDefault();
        }
        #endregion

        #region Helper functions



        private Game GetGame(String gameTitle)
        {
            return _context.Game
                .Where(game => game.Title == gameTitle)
                .Include(game => game.Map)
                .ThenInclude(map => map.Row)
                .ThenInclude(field => field.Trial)
                .ThenInclude(trial => trial.Alternatives)
                .FirstOrDefault();
        }

        private GameplayData GetGameplayData(String userName, String gameTitle)
        {
            return _context.GameplayData
                .Include(data => data.CurrentPlayerPosition)
                .Where(data => data.PlayerName == userName && data.GameTitle == gameTitle)
                .Include(data => data.VisitedFields)
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
            GameplayData dataToDelete = _context.GameplayData.Where(data => data.PlayerName == playerName && data.GameTitle == gameTitle
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
