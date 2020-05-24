using System;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using AdventureGameEditor.Data;
using AdventureGameEditor.Models.Services;
using AdventureGameEditor.Models;
using AdventureGameEditor.Models.DatabaseModels.Game;
using AdventureGameEditor.Models.Enums;
using AdventureGameEditor.Models.ViewModels.Gameplay;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;
using NuGet.Frameworks;
using AdventureGameEditor.Models.DatabaseModels.Gameplay;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Org.BouncyCastle.Crypto.Tls;
using AdventureGameEditor.Models.ViewModels.GameEditor;

namespace AdventureGameEditor.UnitTests
{
    public class GameplayServiceTest : TestUtilities
    {
        private readonly GameplayService gameplayService;

        public GameplayServiceTest():base()
        {
            //Initialize test users.
            //InitializeTestUsers();
            gameplayService = new GameplayService(context);
            context.SaveChanges();
        }


        #region Test initialize gameplay
        /*
        [Fact]
        public void TestGetGameplayViewModel()
        {
            InitializeTestUsers();
            
            Random rnd = new Random();
            // Test on a small map.
            String userName = "TestUserG1";
            String gameTitle = "GameplayWithSmallMap";
            MakeGameWithCorrectMap(userName, gameTitle, Visibility.Everyone);
            Game game = GetTestGame(gameTitle, userName);
            game.StartField = GetField(gameTitle, userName, rnd.Next(0, 3), rnd.Next(0,3));
            game.TargetField = GetField(gameTitle, userName, rnd.Next(0,3), rnd.Next(0,3));
            context.SaveChanges();
            CheckInitializedGamePlayViewModel(gameplayService.GetGameplayViewModel(userName, gameTitle), userName, gameTitle);
            CheckInitializedGameplayData(userName, gameTitle);

            // Test on a large map.
            userName = "TestUserG2";
            gameTitle = "GameplayWithLargeMap";
            MakeGameWithLargeCorrectMap(userName, gameTitle, Visibility.LoggedIn);
            game = GetTestGame(gameTitle, userName);
            game.StartField = GetField(gameTitle, userName, rnd.Next(0, 15), rnd.Next(0, 15));
            context.SaveChanges();
            CheckInitializedGamePlayViewModel(gameplayService.GetGameplayViewModel(userName, gameTitle), userName, gameTitle);
            CheckInitializedGameplayData(userName, gameTitle);

            // Test on some random games.
            MakeUniqueTestGames(10, true);
            for(int i = 50; i < 60; ++i)
            {
                gameTitle = "GameTitle" + i;
                userName = "TestUser" + (i - 50);
                CheckInitializedGamePlayViewModel(gameplayService.GetGameplayViewModel(userName, gameTitle), userName, gameTitle);
                CheckInitializedGameplayData(userName, gameTitle);
            }
        }*/
    
        #endregion

        #region Test step and load game



        #endregion

        #region Helper functions

        #region Check data

        private void CheckInitializedGamePlayViewModel(GameplayViewModel model, String userName, String gameTitle)
        {
            Game game = context.Game.Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                                    .Include( g => g.StartField)
                                    .Include(g => g.TargetField)
                                    .FirstOrDefault();
            Assert.Equal(userName, model.Player.UserName);
            Assert.Equal(gameTitle, model.GameTitle);
            Assert.Equal(game.StartField, model.CurrentPlayerPosition);
            Assert.Equal(game.TargetField, model.TargetField);
            Assert.Equal(0, model.StepCount);
            Assert.False(model.IsGameOver);
            if(game.TableSize <= 5)
            {
                Assert.Equal(3, model.LifeCount);
            }
            else if(game.TableSize <= 10)
            {
                Assert.Equal(5, model.LifeCount);
            }
            else
            {
                Assert.Equal(7, model.LifeCount);
            }
            Assert.False(model.IsVisitiedCurrentPlayerPosition);
        }

        private void CheckInitializedGameplayData(String userName, String gameTitle)
        {
            GameplayData data = context.GameplayData.Where(d => d.PlayerName == userName && d.GameTitle == gameTitle)
                                                    .Include(d => d.CurrentPlayerPosition)
                                                    .Include(d => d.VisitedFields)
                                                    .FirstOrDefault();
            Game game = context.Game.Where(g => g.Title == gameTitle)
                                    .Include(g => g.StartField)
                                    .Include(g => g.TargetField)
                                    .FirstOrDefault();
            Assert.Equal(userName, data.PlayerName);
            Assert.Equal(gameTitle, data.GameTitle);
            Assert.Equal(game.StartField, data.CurrentPlayerPosition);
            Assert.Equal(0, data.StepCount);
            Assert.Equal(GameCondition.OnGoing, data.GameCondition);
            if(game.TableSize <= 5)
            {
                Assert.Equal(3, data.LifeCount);
            }
            else if(game.TableSize <= 10)
            {
                Assert.Equal(5, data.LifeCount);
            }
            else
            {
                Assert.Equal(7, data.LifeCount);
            }
            foreach(IsVisitedField field in data.VisitedFields)
            {
                Assert.False(field.IsVisited);
            }

        }
        #endregion

        #endregion
    }
}



