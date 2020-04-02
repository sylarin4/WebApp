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

        public GameplayService(AdventureGameEditorContext context)
        {
            _context = context;
        }

        public GameplayViewModel GetGameplayViewModel(String userName, String gameTitle)
        {
            GameplayData gameplayData = _context.GameplayData
                .Where(gameplay => gameplay.Player.UserName == userName && gameplay.GameTitle == gameTitle)
                .FirstOrDefault();
            
                Game game = GetGame(userName, gameTitle);
            // Check if we have to initialize.
            if(gameplayData == null)
            {
                gameplayData = new GameplayData()
                {
                    Player = _context.User.Where(user => user.UserName == userName).FirstOrDefault(),
                    GameTitle = gameTitle,
                    CurrentPlayerPosition = game.StartField,
                    StepCount = 0,
                    IsGameOver = false,
                    VisitedFields = InitializeVisitedFields(game.TableSize),
                    StartDate = DateTime.Now,
                    LastPlayDate = DateTime.Now
                };
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
                IsGameOver = gameplayData.IsGameOver,
                GameplayMap = InitializeGameplayMap(game.Map.ToList()),
                StartDate = gameplayData.StartDate,
                LastPlayDate = DateTime.Now
            };

            return gameplayViewModel;
        }

        public List<List<GameplayField>> InitializeGameplayMap(List<MapRow> map)
        {
            List<List<GameplayField>> gameplayMap = new List<List<GameplayField>>();
            foreach(MapRow row in map)
            {
                List<GameplayField> gameplayRow = new List<GameplayField>();
                foreach(Field field in row.Row)
                {
                    gameplayRow.Add(new GameplayField()
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
            return gameplayMap;
        }

        public List<IsVisitedField> InitializeVisitedFields(int mapSize)
        {
            List<IsVisitedField> visitedFields = new List<IsVisitedField>();
            for(int i = 0; i < mapSize; ++i)
            {
                for(int j = 0; j < mapSize; ++j)
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

        #region Helper functions

        private Game GetGame(String userName, String gameTitle)
        {
            return _context.Game
                .Where(game => game.Owner.UserName == userName && game.Title == gameTitle)
                .Include(game => game.Map)
                .ThenInclude(map => map.Row)
                .FirstOrDefault();
        }

        #endregion
    }
}
