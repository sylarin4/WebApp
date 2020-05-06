using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.IO;

using AdventureGameEditor.Data;
using AdventureGameEditor.Models.ViewModels.Gameplay;

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
                //GameplayMap = InitializeGameplayMap(game.Map.ToList()),
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

        public Field StepGame(String userName, String gameTitle, Direction direction)
        {
            GameplayData gameplayData = GetGameplayData(userName, gameTitle);

            // If the direction is not valid, return the current field.
            if (direction == Direction.NotSet) return gameplayData.CurrentPlayerPosition;

            // Set the last field to visited.
            gameplayData.VisitedFields
                .Where(field => field.ColNumber == gameplayData.CurrentPlayerPosition.ColNumber
                    && field.RowNumber == gameplayData.CurrentPlayerPosition.RowNumber)
                .FirstOrDefault()
                .IsVisited = true;

            // Step the player to the next field.
            Field newPosition = GetNextField(gameplayData.CurrentPlayerPosition, GetGame(gameTitle).TableSize, direction);
            int tableSize = GetGame(gameTitle).TableSize;
            gameplayData.CurrentPlayerPosition = newPosition;
            gameplayData.StepCount++;
            gameplayData.LastPlayDate = DateTime.Now;

            // GameCondition just changes here if we are at the target field.
            gameplayData.GameCondition =
                (IsAtTargetField(gameTitle, newPosition.RowNumber, newPosition.ColNumber) 
                && GetTrial(gameTitle, newPosition.ColNumber, newPosition.RowNumber) == null)
                ? GameCondition.Won : GameCondition.OnGoing;
          
            _context.SaveChanges();
            return gameplayData.CurrentPlayerPosition;
        }

        // Returns the next field in the given direction, if it's exists and available.
        // If not, returns the same field.
        public Field GetNextField(Field currentField, int tableSize, Direction direction)
        {
            int resultColNumber = currentField.ColNumber;
            int resultRowNumber = currentField.RowNumber;
            switch (direction)
            {
                case Direction.Up:
                    if (currentField.IsUpWay && currentField.RowNumber > 0)
                        resultRowNumber = currentField.RowNumber - 1;
                    break;
                case Direction.Right:
                    if (currentField.IsRightWay && currentField.ColNumber + 1 < tableSize)
                        resultColNumber = currentField.ColNumber + 1;
                    break;
                case Direction.Down:
                    if (currentField.IsDownWay && currentField.RowNumber + 1 < tableSize)
                        resultRowNumber = currentField.RowNumber + 1;
                    break;
                case Direction.Left:
                    if (currentField.IsLeftWay && currentField.ColNumber > 0)
                        resultColNumber = currentField.ColNumber - 1;
                    break;
            }
            return GetField(currentField.GameTitle, resultRowNumber, resultColNumber);
        }

        public GameResult GetGameResult(String gameTitle, String userName)
        {
            if(GetGameplayData(userName, gameTitle) == null)
            {
                Game game = GetGame(gameTitle);
                return new GameResult()
                {
                    Owner = game.Owner,
                    GameTitle = gameTitle,
                    IsGameWonResult = true,
                    Image = game.GameWonResult != null ? game.GameWonResult.Image : null,
                    Text = game.GameWonResult != null ? game.GameWonResult.Text : ""
                };
            }
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

        #region Do trial
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
                WillTeleport = false,
                GameLost = false,
                GameWon = false,
                IsDownWay = field.IsDownWay,
                IsLeftWay = field.IsLeftWay,
                IsRightWay = field.IsRightWay,
                IsUpWay = field.IsUpWay
            };
            switch (GetTrial(gameTitle, colNumber, rowNumber).Alternatives[trialNumber].TrialResult.ResultType)
            {
                case ResultType.LoseLife:
                    gameplayData.LifeCount--;

                    // If this was the last life, game is lost.
                    if (gameplayData.LifeCount < 1)
                    {
                        // Set game lost.
                        SetGameOver(playerName, gameTitle, false);
                        model.GameLost = true;
                    }
                    _context.SaveChanges();
                    break;
                case ResultType.GameWon:
                    // Set game won.
                    SetGameOver(playerName, gameTitle, true);
                    model.GameWon = true;
                    break;
                case ResultType.Teleport:
                    model.WillTeleport = true;
                    break;
                case ResultType.GetLife:
                    gameplayData.LifeCount++;
                    _context.SaveChanges();
                    break;
                default:
                    if (isAtTargetField)
                    {
                        // Set game won.
                        SetGameOver(playerName, gameTitle, true);
                        model.GameWon = true;
                    }
                    break;
            }
            if( ! model.GameWon && !model.GameLost && ! model.WillTeleport && isAtTargetField)
            {
                SetGameOver(playerName, gameTitle, true);
                model.GameWon = true;
            }
            return model;
        }

        public CompassPoint GetTargetDirection(String gameTitle, Field field)
        {
            Field targetField = GetGame(gameTitle).TargetField;

            // Check if target field is in direction North from the field.
            if(field.RowNumber > targetField.RowNumber && field.ColNumber == targetField.ColNumber)
                return CompassPoint.North;

            // Check if target field is in direction South from the field.
            if(field.RowNumber < targetField.RowNumber && field.ColNumber == targetField.ColNumber)
                return CompassPoint.South;

            // Check if target field is in direction East from the field.
            if (field.RowNumber == targetField.RowNumber && field.ColNumber < targetField.ColNumber)
                return CompassPoint.East;

            // Check if target field is in direction West from the field.
            if (field.RowNumber == targetField.RowNumber && field.ColNumber > targetField.ColNumber)
                return CompassPoint.West;

            // Check if target field is in direction Nort-East from the field.
            if (field.RowNumber > targetField.RowNumber && field.ColNumber < targetField.ColNumber)
                return CompassPoint.NorthEast;

            // Check if target field is in direction Nort-West from the field.
            if (field.RowNumber > targetField.RowNumber && field.ColNumber > targetField.ColNumber)
                return CompassPoint.NorthWest;

            // Check if target field is in direction South-East from the field.
            if (field.RowNumber < targetField.RowNumber && field.ColNumber < targetField.ColNumber)
                return CompassPoint.SouthEast;

            // Check if target field is in direction South-West from the field.
            if (field.RowNumber < targetField.RowNumber && field.ColNumber > targetField.ColNumber)
                return CompassPoint.SouthWest;

            // Check if the field is the target field.
            if (field.RowNumber == targetField.RowNumber && field.ColNumber == targetField.ColNumber)
                return CompassPoint.Here;

            // If didn't find the direction (but it isn't possible) return no direction.
            return CompassPoint.NoDirection;
        }
        #endregion

        #region Teleport

        public Direction GetRandDirection()
        {
            Random randGen = new Random();
            int randNum = randGen.Next(1, 5);
            switch (randNum)
            {
                case 1: return Direction.Up;
                case 2: return Direction.Down;
                case 3: return Direction.Left;
                case 4: return Direction.Right;
                default: return Direction.NotSet;
            }
        }

        public int GetGameMapSize(String gameTitle)
        {
            return GetGame(gameTitle).TableSize;
        }

        #endregion

        #region Usually used getters

        public GameplayFieldViewModel GetGameplayFieldViewModel(String playerName, String gameTitle, Field field)
        {
            return new GameplayFieldViewModel()
            {
                GameTitle = gameTitle,
                RowNumber = field.RowNumber,
                ColNumber = field.ColNumber,
                Text = field.Text,
                FieldImageID = field.Image != null ? field.Image.ID : -1,
                Trial = field.Trial,
                IsRightWay = field.IsRightWay,
                IsLeftWay = field.IsLeftWay,
                IsUpWay = field.IsUpWay,
                IsDownWay = field.IsDownWay,
                IsVisited = GetIsVisitedField(playerName, gameTitle,
                    field.ColNumber, field.RowNumber),
                IsAtTargetField = IsAtTargetField(gameTitle, field.RowNumber, field.ColNumber),
                LifeCount = GetLifeCount(playerName, gameTitle)
            };
        }

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
                .Include(game =>game.GameWonResult)
                .ThenInclude(result=> result.Image)
                .Include(game=>game.GameLostResult)
                .ThenInclude(result=>result.Image)
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
