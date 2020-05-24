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
using AdventureGameEditor.Models.ViewModels.GameEditor;
using AdventureGameEditor.Models.DatabaseModels.Gameplay;
using AdventureGameEditor.Models.ViewModels.Gameplay;
using System.Threading;
using SQLitePCL;
using Microsoft.EntityFrameworkCore.Internal;
using MySqlX.XDevAPI.Relational;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Asn1.X509;

namespace AdventureGameEditor.UnitTests
{

    public class AdventureGameEditorTest : IDisposable
    {
        private readonly AdventureGameEditorContext _context;
        private readonly IGameEditorService gameEditorService;
        private readonly GameplayService gameplayService;

        public AdventureGameEditorTest()
        {
            // Initialize context.
            var contextOptions = new DbContextOptionsBuilder<AdventureGameEditorContext>()
                .UseInMemoryDatabase("AdventureGameEditorContext")
                .Options;
            _context = new AdventureGameEditorContext(contextOptions);
            _context.Database.EnsureCreated();
            gameEditorService = new GameEditorService(_context);
            gameplayService = new GameplayService(_context);
            InitializeTestUsers();
            _context.SaveChanges();
        }
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        //===--------- Test GameEditorService's functions ----------===//

        #region Test game creation
        [Fact]
        public void TestGameInitialization()
        {
            for (int i = 3; i < 16; ++i)
            {
                CheckGameInitialization("GameTitle" + i, "TestUser" + i, Visibility.Owner, i);
                CheckGameInitialization("GameTitile" + 1 + i, "TestUser" + i, Visibility.LoggedIn, i);
                CheckGameInitialization("GameTitile" + 2 + i, "TestUser" + i, Visibility.Everyone, i);
            }
        }

        private void CheckGameInitialization(String gameTitle, String userName, Visibility visibility, int tableSize)
        {
            InitializeTestUsers();
            gameEditorService.InicializeGame(gameTitle, tableSize, visibility,
                _context.User.Where(u => u.UserName == userName).FirstOrDefault(), null);
            Game game = _context.Game.Where(g => g.Title == gameTitle)
                .Include(g => g.Map)
                .ThenInclude(m => m.Row)
                .ThenInclude(r => r.Trial)
                .FirstOrDefault();

            Assert.NotNull(game);

            Assert.Equal(_context.User.Where(u => u.UserName == userName).FirstOrDefault(), game.Owner);
            Assert.Equal(gameTitle, game.Title);
            Assert.Equal(visibility, game.Visibility);
            Assert.Equal(0, game.PlayCounter);
            Assert.Equal(tableSize, game.TableSize);


            CheckDefaultMap(game.Map, userName, gameTitle);
        }
        #endregion

        #region Test map creation

        #region Getters

        [Fact]
        public void TestGetMapViewModel()
        {
            // Check on default games. (with no filled fields)
            MakeDefaultTestGames();
            for (int i = 0; i < 50; ++i)
            {
                MapViewModel mapViewModel = gameEditorService.GetMapViewModel("TestUser" + i, "GameTitle" + i);
                Assert.Equal("GameTitle" + i, mapViewModel.GameTitle);
                Assert.Equal(GetTestGame("GameTitle" + i, "TestUser" + i).TableSize, mapViewModel.MapSize);
                CheckDefaultMap(mapViewModel.Map, "TestUser" + i, "GameTitle" + i);
            }

            // Check on unique game.
            MakeTestGame("UniqueGameTitle", 15, Visibility.Everyone, "TestUser10");
            MapViewModel mapViewModel2 = gameEditorService.GetMapViewModel("TestUser10", "UniqueGameTitle");
            CheckMapViewModelMap(mapViewModel2.Map, "TestUser10", "UniqueGameTitle");

            //Check on more unigue games.
            MakeUniqueTestGames(30, false);
            for (int i = 50; i < 80; ++i)
            {
                mapViewModel2 = gameEditorService.GetMapViewModel("TestUser" + (i - 50), "GameTitle" + i);
                CheckMapViewModelMap(mapViewModel2.Map, "TestUser" + (i - 50), "GameTitle" + i);
            }
        }

        [Fact]
        public void TestGetFieldViewModel()
        {
            // Make some games for testing.
            MakeDefaultTestGames();
            MakeUniqueTestGames(10, false);
            for (int i = 0; i < 10; i++)
            {
                ICollection<MapRow> map = GetMap("TestUser" + i, "GameTitle" + i);
                foreach (MapRow mapRow in map)
                {
                    foreach (Field field in mapRow.Row)
                    {
                        MapPieceViewModel fieldViewModel = gameEditorService.GetFieldViewModel("TestUser" + i, "GameTitle" + i,
                            field.RowNumber, field.ColNumber);
                        Assert.Equal(field.GameTitle, fieldViewModel.GameTitle);
                        CheckField(fieldViewModel.Field, "TestUser" + i, "GameTitle" + i);
                    }
                }
            }
            for (int i = 50; i < 60; i++)
            {
                ICollection<MapRow> map = GetMap("TestUser" + (i - 50), "GameTitle" + i);
                foreach (MapRow mapRow in map)
                {
                    foreach (Field field in mapRow.Row)
                    {
                        MapPieceViewModel fieldViewModel = gameEditorService.GetFieldViewModel("TestUser" + (i - 50), "GameTitle" + i,
                            field.RowNumber, field.ColNumber);
                        Assert.Equal(field.GameTitle, fieldViewModel.GameTitle);
                        CheckField(fieldViewModel.Field, "TestUser" + (i - 50), "GameTitle" + i);
                    }
                }
            }
        }

        [Fact]
        public void TestGetTextAtCoordinate()
        {
            MakeUniqueTestGames(10, true);
            for (int i = 50; i < 60; ++i)
            {
                ICollection<MapRow> map = GetMap("TestUser" + (i - 50), "GameTitle" + i);
                foreach (MapRow mapRow in map)
                {
                    foreach (Field field in mapRow.Row)
                    {
                        String fieldText = gameEditorService.GetTextAtCoordinate("TestUser" + (i - 50), "GameTitle" + i,
                            field.RowNumber, field.ColNumber);
                        Assert.Equal(field.Text, fieldText);
                    }
                }
            }
        }

        #endregion

        #region Setters

        [Fact]
        public void TestSetExitRoads()
        {
            // Test on a game with the minimum map size.
            MakeTestGame("GameTitleMin", 3, Visibility.Everyone, "TestUser1");
            Game game = GetTestGame("GameTitleMin", "TestUser1");

            foreach (MapRow mapRow in game.Map)
            {
                foreach (Field field in mapRow.Row)
                {
                    TestFieldOnEveryTypeOfWayDirectionsCode(game, field.RowNumber, field.ColNumber, "GameTitleMin",
                        "TestUser1");
                }
            }

            // Test on a game with the maximum map size.
            MakeTestGame("GameTitleMax", 15, Visibility.LoggedIn, "TestUser2");
            Game game2 = GetTestGame("GameTitleMax", "TestUser2");

            foreach (MapRow mapRow in game2.Map)
            {
                foreach (Field field in mapRow.Row)
                {
                    if (field != null)
                    {
                        TestFieldOnEveryTypeOfWayDirectionsCode(game2, field.RowNumber, field.ColNumber, "GameTitleMax",
                           "TestUser2");

                    }
                }
            }

            // Test on a game with average map size.
            MakeTestGame("GameTitleAv", 7, Visibility.Owner, "TestUser3");
            Game game3 = GetTestGame("GameTitleAv", "TestUser3");

            foreach (MapRow mapRow in game3.Map)
            {
                foreach (Field field in mapRow.Row)
                {
                    TestFieldOnEveryTypeOfWayDirectionsCode(game3, field.RowNumber, field.ColNumber, "GameTitleAv",
                        "TestUser3");
                }
            }

            // Test on some games with random map size.
            Random rnd = new Random();
            int random = 3;
            for (int i = 0; i < 5; ++i)
            {
                random = rnd.Next(3, 16);
                MakeTestGame("GameTitle" + i, random, Visibility.Owner, "TestUser" + i);
                game = GetTestGame("GameTitle" + i, "TestUser" + i);

                foreach (MapRow mapRow in game.Map)
                {
                    foreach (Field field in mapRow.Row)
                    {
                        TestFieldOnEveryTypeOfWayDirectionsCode(game, field.RowNumber, field.ColNumber, "GameTitle" + i,
                            "TestUser" + i);
                    }
                }
            }
        }

        [Fact]
        public void TestSetCurrentWayDirectionsCode()
        {
            String userName = "TestUser11";
            String gameTitle = "GameTitle";
            MakeTestGame(gameTitle, 6, Visibility.Everyone, userName);

            //---------- Test with all type of way directions code ----------//

            // Test with 0110 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 0110);
            Assert.Equal(0110, GetWayDirectionsCode(gameTitle, userName));

            // Test with 0111 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 0111);
            Assert.Equal(0111, GetWayDirectionsCode(gameTitle, userName));

            // Test with 0011 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 0011);
            Assert.Equal(0011, GetWayDirectionsCode(gameTitle, userName));



            // Test with 1110 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 1110);
            Assert.Equal(1110, GetWayDirectionsCode(gameTitle, userName));

            // Test with 1111 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 1111);
            Assert.Equal(1111, GetWayDirectionsCode(gameTitle, userName));

            // Test with 1011 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 1011);
            Assert.Equal(1011, GetWayDirectionsCode(gameTitle, userName));



            // Test with 1100 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 1100);
            Assert.Equal(1100, GetWayDirectionsCode(gameTitle, userName));

            // Test with 1101 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 1101);
            Assert.Equal(1101, GetWayDirectionsCode(gameTitle, userName));

            // Test with 1001 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 1001);
            Assert.Equal(1001, GetWayDirectionsCode(gameTitle, userName));



            // Test with 0100 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 0100);
            Assert.Equal(0100, GetWayDirectionsCode(gameTitle, userName));

            // Test with 0101 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 0101);
            Assert.Equal(0101, GetWayDirectionsCode(gameTitle, userName));

            // Test with 0001 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 0001);
            Assert.Equal(0001, GetWayDirectionsCode(gameTitle, userName));



            // Test with 0010 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 0010);
            Assert.Equal(0010, GetWayDirectionsCode(gameTitle, userName));

            // Test with 1000 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 1000);
            Assert.Equal(1000, GetWayDirectionsCode(gameTitle, userName));

            // Test with 1010 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 1010);
            Assert.Equal(1010, GetWayDirectionsCode(gameTitle, userName));

            // Test with 0000 way directions code.
            gameEditorService.SetCurrentWayDirectionsCode(userName, gameTitle, 0000);
            Assert.Equal(0000, GetWayDirectionsCode(gameTitle, userName));
        }

        #endregion

        #endregion

        #region Test create map content

        #region Setters

        [Fact]
        public void TestInitializeAlternativeTexts()
        {
            List<String> initializedAlternativeTexts = gameEditorService.InitializeAlternativeTexts(4);
            for (int i = 0; i < 4; ++i)
            {
                Assert.Empty(initializedAlternativeTexts[i]);
            }
        }

        [Fact]
        public void TestInitializeTrialResults()
        {
            List<TrialResult> trialResults = gameEditorService.InitializeTrialResults(4);
            for (int i = 0; i < 4; ++i)
            {
                Assert.Equal(ResultType.Nothing, trialResults[i].ResultType);
                Assert.Empty(trialResults[i].Text);
            }
        }

        [Fact]
        public void TestAddTextAndImageForField()
        {
            MakeUniqueTestGames(10, false);

            // Make a long string for testing
            String longString = "";
            for (int i = 0; i < 100; ++i)
            {
                longString += "It's a long string for testing.";
            }
            for (int i = 50; i < 60; ++i)
            {
                ICollection<MapRow> map = GetMapWhitoutTrials("TestUser" + (i - 50), "GameTitle" + i);
                String userName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;
                foreach (MapRow mapRow in map) {
                    foreach (Field field in mapRow.Row)
                    {
                        // Test on empty strings.
                        gameEditorService.AddTextAndImageForField(userName, gameTitle, field.RowNumber,
                            field.ColNumber, "", null);
                        Assert.Empty(GetField(gameTitle, userName, field.RowNumber, field.ColNumber).Text);

                        // Test on long strings.
                        gameEditorService.AddTextAndImageForField(userName, gameTitle, field.RowNumber,
                            field.ColNumber, longString, null);
                        Assert.Equal(longString, GetField(gameTitle, userName, field.RowNumber, field.ColNumber).Text);
                        // Test on average strings.
                        gameEditorService.AddTextAndImageForField(userName, gameTitle, field.RowNumber, field.ColNumber,
                            "This isn't a very long string or a very short one, but this is unique." + field.ColNumber + field.RowNumber
                            , null);
                        Assert.Equal("This isn't a very long string or a very short one, but this is unique." + field.ColNumber + field.RowNumber,
                            GetField(gameTitle, userName, field.RowNumber, field.ColNumber).Text);

                    }
                }
            }
        }

        [Fact]
        public void TestSaveTrial()
        {
            MakeUniqueTestGames(5, true);

            for (int i = 50; i < 55; ++i)
            {
                String userName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;
                ICollection<MapRow> map = GetMap(userName, gameTitle);
                foreach (MapRow mapRow in map)
                {
                    foreach (Field field in mapRow.Row)
                    {
                        // Test with empty trial text.
                        List<Alternative> alternatives = GenerateAlternatives();
                        String text = "";
                        Tuple<List<String>, List<TrialResult>> textsAndTrialResults = GetTextsAndTrialResultsFromAlternatives(alternatives);
                        gameEditorService.SaveTrial(userName, gameTitle, field.RowNumber, field.ColNumber,
                            textsAndTrialResults.Item1, textsAndTrialResults.Item2, TrialType.MultipleChoice, text);
                        CheckTrial(alternatives, text, GetField(gameTitle, userName, field.RowNumber, field.ColNumber));

                        // Test with long trial text.
                        alternatives = GenerateAlternatives();
                        text = GetLongTestString();
                        textsAndTrialResults = GetTextsAndTrialResultsFromAlternatives(alternatives);
                        gameEditorService.SaveTrial(userName, gameTitle, field.RowNumber, field.ColNumber,
                            textsAndTrialResults.Item1, textsAndTrialResults.Item2, TrialType.MultipleChoice, text);
                        CheckTrial(alternatives, text, GetField(gameTitle, userName, field.RowNumber, field.ColNumber));

                        // Test with average long trial text.
                        alternatives = GenerateAlternatives();
                        text = GetAverageTestString();
                        textsAndTrialResults = GetTextsAndTrialResultsFromAlternatives(alternatives);
                        gameEditorService.SaveTrial(userName, gameTitle, field.RowNumber, field.ColNumber,
                            textsAndTrialResults.Item1, textsAndTrialResults.Item2, TrialType.MultipleChoice, text);
                        CheckTrial(alternatives, text, GetField(gameTitle, userName, field.RowNumber, field.ColNumber));
                    }
                }
            }
        }

        #endregion

        #region Getters

        [Fact]
        public void TestGetTrial()
        {
            MakeUniqueTestGames(10, true);
            for (int i = 50; i < 60; ++i)
            {
                String userName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;
                ICollection<MapRow> map = GetMap(userName, gameTitle);
                foreach (MapRow mapRow in map)
                {
                    foreach (Field field in mapRow.Row)
                    {
                        field.Trial = GenerateTrial();
                        _context.SaveChanges();
                        Trial trial = gameEditorService.GetTrial(userName, gameTitle, field.ColNumber, field.RowNumber);
                        CheckTrial(trial.Alternatives, trial.Text, field);
                    }
                }
            }
        }

        [Fact]
        public void TestGetMapContentViewModel()
        {
            MakeUniqueTestGames(50, true);
            for (int i = 50; i < 100; ++i)
            {
                String userName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;
                MapContentViewModel viewModel = gameEditorService.GetMapContentViewModel(userName, gameTitle);
                Assert.Equal(gameTitle, viewModel.GameTitle);
                Assert.Equal(GetTestGame(gameTitle, userName).TableSize, viewModel.MapSize);
                CheckEditedMap(viewModel.Map, userName, gameTitle);
            }
        }

        [Fact]
        public void TestGetFieldTrialContentViewModel()
        {
            MakeUniqueTestGames(10, true);
            for (int i = 50; i < 60; ++i)
            {
                String gameTitle = "GameTitle" + i;
                String userName = "TestUser" + (i - 50);
                ICollection<MapRow> map = GetMap(userName, gameTitle);
                foreach (MapRow mapRow in map)
                {
                    foreach (Field field in mapRow.Row)
                    {
                        field.Trial = GenerateTrial();
                        _context.SaveChanges();
                        FieldTrialContentViewModel viewModel = gameEditorService.GetFieldTrialContentViewModel
                            (userName, gameTitle, field.RowNumber, field.ColNumber);
                        Assert.Equal(gameTitle, viewModel.GameTitle);
                        Assert.Equal(field.ColNumber, viewModel.ColNumber);
                        Assert.Equal(field.RowNumber, viewModel.RowNumber);
                        CheckTrial(GetAlternativesFromTextsAndTrialResults(viewModel.AlternativeTexts,
                            viewModel.TrialResults), viewModel.Text, field);
                    }
                }

            }
        }

        #endregion

        #endregion

        #region Test set start and target field

        [Fact]
        public void TestSaveStartField()
        {
            MakeUniqueTestGames(10, true);
            for (int i = 50; i < 60; ++i)
            {
                String gameTitle = "GameTitle" + i;
                String userName = "TestUser" + (i - 50);
                ICollection<MapRow> map = GetMap(userName, gameTitle);
                foreach (MapRow mapRow in map)
                {
                    foreach (Field field in mapRow.Row)
                    {
                        gameEditorService.SaveStartField(userName, gameTitle, field.RowNumber, field.ColNumber);
                        Game game = _context.Game
                                        .Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                                        .Include(g => g.StartField)
                                        .FirstOrDefault();
                        Assert.Equal(field.RowNumber, game.StartField.RowNumber);
                        Assert.Equal(field.ColNumber, game.StartField.ColNumber);
                        CheckField(game.StartField, userName, gameTitle);
                    }
                }
            }
        }

        [Fact]
        public void TestSaveTargetField()
        {
            MakeUniqueTestGames(10, true);
            for (int i = 50; i < 60; ++i)
            {
                String gameTitle = "GameTitle" + i;
                String userName = "TestUser" + (i - 50);
                ICollection<MapRow> map = GetMap(userName, gameTitle);
                Assert.NotNull(map);
                Assert.NotNull(GetTestGame(gameTitle, userName));
                foreach (MapRow mapRow in map)
                {
                    foreach (Field field in mapRow.Row)
                    {
                        gameEditorService.SaveTargetField(userName, gameTitle, field.RowNumber, field.ColNumber);
                        Game game = _context.Game
                                        .Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                                        .Include(g => g.TargetField)
                                        .FirstOrDefault();
                        Assert.Equal(field.RowNumber, game.TargetField.RowNumber);
                        Assert.Equal(field.ColNumber, game.TargetField.ColNumber);
                        CheckField(game.TargetField, userName, gameTitle);
                    }
                }
            }
        }

        #endregion

        #region Test show game details

        [Fact]
        public void TestGetGameDetailsViewModel()
        {
            MakeUniqueTestGames(50, true);
            for (int i = 50; i < 100; ++i)
            {
                String gameTitle = "GameTitle" + i;
                String userName = "TestUser" + (i - 50);
                GameDetailsViewModel model = gameEditorService.GetGameDetailsViewModel(userName, gameTitle);
                Game game = GetTestGame(gameTitle, userName);
                Assert.Equal(game.Owner.UserName, model.OwnerName);
                Assert.Equal(game.Title, model.Title);
                Assert.Equal(game.Visibility, model.Visibility);
                Assert.Equal(game.TableSize, model.TableSize);
                CheckMapViewModelMap(model.Map, userName, gameTitle);
                Assert.Equal(game.StartField, model.StartField);
                Assert.Equal(game.TargetField, model.TargetField);
                Assert.Equal(game.GameLostResult.Text, model.GameLostResult);
                Assert.Equal(game.GameWonResult.Text, model.GameWonResult);
                Assert.Equal(game.Prelude.Text, model.Prelude);
                Assert.Equal(game.Summary, model.Summary);

            }
        }

        [Fact]
        public void TestGetFieldDetailsViewModel()
        {
            MakeUniqueTestGames(20, true);
            for (int i = 50; i < 70; ++i)
            {
                String userName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;
                ICollection<MapRow> map = GetMap(userName, gameTitle);
                foreach (MapRow mapRow in map)
                {
                    foreach (Field field in mapRow.Row)
                    {
                        if (field.ColNumber % 3 == 0)
                        {
                            field.Trial = GenerateTrial();
                            _context.SaveChanges();
                        }
                        FieldDetailsViewModel model = gameEditorService.GetFieldDetailsViewModel(userName, gameTitle,
                            field.ColNumber, field.RowNumber);
                        Assert.Equal(field.ColNumber, model.ColNumber);
                        Assert.Equal(field.RowNumber, model.RowNumber);
                        Assert.Equal(field.Text, model.TextContent);
                        Assert.Equal(field.Trial != null, model.IsTrial);
                        if (field.Trial != null)
                        {
                            Assert.Equal(field.Trial.TrialType, model.TrialType);
                            Assert.Equal(field.Trial.Text, model.TrialText);
                            for (int j = 0; j < 4; ++j)
                            {
                                Assert.Equal(field.Trial.Alternatives[j].Text, model.AlternativeTexts[j]);
                                Assert.Equal(field.Trial.Alternatives[j].TrialResult.Text, model.TrialResults[j].Text);
                                Assert.Equal(field.Trial.Alternatives[j].TrialResult.ResultType, model.TrialResults[j].ResultType);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Test create game result

        [Fact]
        public void TestSaveGameResults()
        {
            MakeUniqueTestGames(50, true);
            for (int i = 50; i < 100; ++i)
            {
                String userName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;

                // Test with empty strings.
                String gameWonResult = "";
                String gameLostResult = "";
                String prelude = "";
                String summary = "";
                Boolean succeeded = gameEditorService.SaveGameResults(userName, gameTitle, gameWonResult, gameLostResult, prelude, null, null, null,
                    null, summary);
                Assert.False(succeeded);

                // Test with average long strings.
                gameWonResult = GetAverageTestString();
                gameLostResult = GetAverageTestString();
                prelude = GetAverageTestString();
                summary = GetAverageTestString();
                succeeded = gameEditorService.SaveGameResults(userName, gameTitle, gameWonResult, gameLostResult, prelude, null,
                    null, null, null, summary);
                Assert.True(succeeded);
                CheckGameResults(userName, gameTitle, gameWonResult, gameLostResult, prelude, summary);

                // Test with long strings.
                gameWonResult = GetLongTestString();
                gameLostResult = GetLongTestString();
                prelude = GetLongTestString();
                summary = GetLongTestString();
                gameEditorService.SaveGameResults(userName, gameTitle, gameWonResult, gameLostResult, prelude, null,
                    null, null, null, summary);
                Assert.True(succeeded);
                CheckGameResults(userName, gameTitle, gameWonResult, gameLostResult, prelude, summary);

            }
        }

        [Fact]
        public void TestGetGameResult()
        {
            MakeUniqueTestGames(50, true);
            for (int i = 50; i < 100; ++i)
            {
                String gameTitle = "GameTitle" + i;
                String userName = "TestUser" + (i - 50);
                Game game = GetTestGame(gameTitle, userName);

                // Test with empty strings.
                game.GameLostResult = GenerateGameResult(userName, gameTitle, false, "");
                game.GameWonResult = GenerateGameResult(userName, gameTitle, true, "");
                game.Prelude = GeneratePrelude(userName, gameTitle, "");
                game.Summary = "";
                _context.SaveChanges();
                CheckGameResultViewModel(gameTitle, userName, gameEditorService.GetGameResult(userName, gameTitle));

                // Test with average long test srting.
                game.GameLostResult = GenerateGameResult(userName, gameTitle, false, GetAverageTestString());
                game.GameWonResult = GenerateGameResult(userName, gameTitle, true, GetAverageTestString());
                game.Prelude = GeneratePrelude(userName, gameTitle, GetAverageTestString());
                game.Summary = GetAverageTestString();
                _context.SaveChanges();
                CheckGameResultViewModel(gameTitle, userName, gameEditorService.GetGameResult(userName, gameTitle));

                // Test with long strings.
                game.GameLostResult = GenerateGameResult(userName, gameTitle, false, GetLongTestString());
                game.GameWonResult = GenerateGameResult(userName, gameTitle, true, GetLongTestString());
                game.Prelude = GeneratePrelude(userName, gameTitle, GetLongTestString());
                game.Summary = GetLongTestString();
                _context.SaveChanges();
                CheckGameResultViewModel(gameTitle, userName, gameEditorService.GetGameResult(userName, gameTitle));
            }
        }

        #endregion

        #region Test game checking

        [Fact]
        public void TestIsValidMap()
        {
            //===---------- Test on a correct maps. ----------===//

            // On a small one.
            String gameTitle = "CorrectGameSmall";
            String userName = "TestUser1";
            MakeGameWithCorrectMap(userName, gameTitle, Visibility.Everyone);
            Boolean succeeded = gameEditorService.IsValidMap(gameTitle, userName);
            Assert.True(succeeded);

            // On a large one.
            gameTitle = "CorrectGameLarge";
            userName = "TestUser15";
            MakeGameWithLargeCorrectMap(userName, gameTitle, Visibility.LoggedIn);
            succeeded = gameEditorService.IsValidMap(gameTitle, userName);
            Assert.True(succeeded);


            //===---------- Test on incorrect maps. ----------===//

            // There's a way that goes out of the map.

            // Small map.
            gameTitle = "IncorrectGame1";
            userName = "TestUser2";
            MakeGameWithCorrectMap(userName, gameTitle, Visibility.LoggedIn);
            GetField(gameTitle, userName, 0, 0).IsLeftWay = true;
            _context.SaveChanges();
            succeeded = gameEditorService.IsValidMap(gameTitle, userName);
            Assert.False(succeeded);

            // Large map.
            gameTitle = "IncorrectGameLarge1";
            userName = "TestUser40";
            MakeGameWithLargeCorrectMap(userName, gameTitle, Visibility.Everyone);
            GetField(gameTitle, userName, 14, 7).IsDownWay = true;
            _context.SaveChanges();
            succeeded = gameEditorService.IsValidMap(gameTitle, userName);
            Assert.False(succeeded);

            // There's map pieces that doesn't fit to each other.

            // Small map.
            gameTitle = "InCorrectGame2";
            userName = "TestUser3";
            MakeGameWithCorrectMap(userName, gameTitle, Visibility.Owner);
            GetField(gameTitle, userName, 1, 1).IsRightWay = false;
            _context.SaveChanges();
            succeeded = gameEditorService.IsValidMap(gameTitle, userName);
            Assert.False(succeeded);

            // Large map.
            gameTitle = "InCorrectGameLarge2";
            userName = "TestUser30";
            MakeGameWithLargeCorrectMap(userName, gameTitle, Visibility.Owner);
            GetField(gameTitle, userName, 13, 13).IsRightWay = false;
            _context.SaveChanges();
            succeeded = gameEditorService.IsValidMap(gameTitle, userName);
            Assert.False(succeeded);

        }

        [Fact]
        public void TestIsStartFieldSet()
        {
            // Test with no start field set.
            String userName = "TestUser49";
            String gameTitle = "StartFieldNotSet";
            MakeTestGame(gameTitle, 15, Visibility.Everyone, userName);
            Boolean succeeded = gameEditorService.IsStartFieldSet(userName, gameTitle);
            Assert.False(succeeded);

            // Test with invalid start field.
            userName = "TestUser44";
            gameTitle = "InvalidStartField";
            MakeTestGame(gameTitle, 15, Visibility.Everyone, userName);
            ICollection<MapRow> map = GetMap(userName, gameTitle);
            foreach (MapRow mapRow in map)
            {
                foreach (Field field in mapRow.Row)
                {
                    field.IsDownWay = false;
                    field.IsLeftWay = false;
                    field.IsRightWay = false;
                    field.IsUpWay = false;
                }
            }
            SetStartField(userName, gameTitle, 13, 13);
            succeeded = gameEditorService.IsStartFieldSet(userName, gameTitle);
            Assert.False(succeeded);

            // Test with valid start field.
            userName = "TestUser41";
            gameTitle = "ValidStartField";
            MakeGameWithLargeCorrectMap(userName, gameTitle, Visibility.LoggedIn);
            SetStartField(userName, gameTitle, 12, 11);
            succeeded = gameEditorService.IsStartFieldSet(userName, gameTitle);
            Assert.True(succeeded);
        }

        [Fact]
        public void TestIsTargetFieldSet()
        {
            // Test with no target field set.
            String userName = "TestUser49";
            String gameTitle = "TargetFieldNotSet";
            MakeTestGame(gameTitle, 15, Visibility.Everyone, userName);
            Boolean succeeded = gameEditorService.IsTargetFieldSet(userName, gameTitle);
            Assert.False(succeeded);

            // Test with invalid target field.
            userName = "TestUser44";
            gameTitle = "InvalidTargetField";
            MakeTestGame(gameTitle, 15, Visibility.Everyone, userName);
            ICollection<MapRow> map = GetMap(userName, gameTitle);
            foreach (MapRow mapRow in map)
            {
                foreach (Field field in mapRow.Row)
                {
                    field.IsDownWay = false;
                    field.IsLeftWay = false;
                    field.IsRightWay = false;
                    field.IsUpWay = false;
                }
            }
            SetTargetField(userName, gameTitle, 13, 13);
            succeeded = gameEditorService.IsTargetFieldSet(userName, gameTitle);
            Assert.False(succeeded);

            // Test with valid target field.
            userName = "TestUser41";
            gameTitle = "ValidTargetField";
            MakeGameWithLargeCorrectMap(userName, gameTitle, Visibility.LoggedIn);
            SetTargetField(userName, gameTitle, 12, 11);
            succeeded = gameEditorService.IsTargetFieldSet(userName, gameTitle);
            Assert.True(succeeded);

        }

        [Fact]
        public void TestIsPreludeFilled()
        {
            // Test with no prelude.
            String userName = "TestUser21";
            String gameTitle = "NotFilled";
            MakeTestGame(gameTitle, 14, Visibility.Everyone, userName);
            Boolean succeeded = gameEditorService.IsPreludeFilled(userName, gameTitle);
            Assert.False(succeeded);

            // Test with a prelude, that have no text.
            userName = "TestUser33";
            gameTitle = "IncorrectPrelude";
            MakeTestGame(gameTitle, 5, Visibility.LoggedIn, userName);
            GetTestGame(gameTitle, userName).Prelude = new Prelude()
            {
                Text = null,
                GameTitle = gameTitle,
                Owner = GetUser(userName)
            };
            _context.SaveChanges();
            succeeded = gameEditorService.IsPreludeFilled(userName, gameTitle);
            Assert.False(succeeded);

            // Test with a prelude that have empty text.
            userName = "TestUser22";
            gameTitle = "IncorrectPrelude2";
            MakeTestGame(gameTitle, 11, Visibility.LoggedIn, userName);
            GetTestGame(gameTitle, userName).Prelude = new Prelude()
            {
                Text = "",
                GameTitle = gameTitle,
                Owner = GetUser(userName)
            };
            _context.SaveChanges();
            succeeded = gameEditorService.IsPreludeFilled(userName, gameTitle);
            Assert.False(succeeded);

            // Test with correct prelude.
            userName = "TestUser22";
            gameTitle = "IncorrectPrelude2";
            MakeTestGame(gameTitle, 11, Visibility.LoggedIn, userName);
            GetTestGame(gameTitle, userName).Prelude = GeneratePrelude(userName, gameTitle, GetLongTestString());
            _context.SaveChanges();
            succeeded = gameEditorService.IsPreludeFilled(userName, gameTitle);
            Assert.True(succeeded);
        }

        [Fact]
        public void TestIsGameWonFilled()
        {
            // Test with no game won result.
            String userName = "TestUser21";
            String gameTitle = "NotFilled";
            MakeTestGame(gameTitle, 14, Visibility.Everyone, userName);
            Boolean succeeded = gameEditorService.IsGameWonFilled(userName, gameTitle);
            Assert.False(succeeded);

            // Test with a game won result, that have no text.
            userName = "TestUser33";
            gameTitle = "IncorrectGameWonResult";
            MakeTestGame(gameTitle, 5, Visibility.LoggedIn, userName);
            GetTestGame(gameTitle, userName).GameWonResult = new GameResult()
            {
                Text = null,
                GameTitle = gameTitle,
                Owner = GetUser(userName),
                IsGameWonResult = true
            };
            _context.SaveChanges();
            succeeded = gameEditorService.IsGameWonFilled(userName, gameTitle);
            Assert.False(succeeded);

            // Test with a game won result that have empty text.
            userName = "TestUser22";
            gameTitle = "IncorrectGameWonResult2";
            MakeTestGame(gameTitle, 11, Visibility.LoggedIn, userName);
            GetTestGame(gameTitle, userName).GameWonResult = new GameResult()
            {
                Text = "",
                GameTitle = gameTitle,
                Owner = GetUser(userName),
                IsGameWonResult = true
            };
            _context.SaveChanges();
            succeeded = gameEditorService.IsGameWonFilled(userName, gameTitle);
            Assert.False(succeeded);

            // Test with correct game won result.
            userName = "TestUser22";
            gameTitle = "IncorrectGameWonResult2";
            MakeTestGame(gameTitle, 11, Visibility.LoggedIn, userName);
            GetTestGame(gameTitle, userName).GameWonResult = GenerateGameResult(userName, gameTitle, true, GetLongTestString());
            _context.SaveChanges();
            succeeded = gameEditorService.IsGameWonFilled(userName, gameTitle);
            Assert.True(succeeded);
        }

        [Fact]
        public void TestIsGameLostFilled()
        {
            // Test with no game lost result.
            String userName = "TestUser21";
            String gameTitle = "NotFilled";
            MakeTestGame(gameTitle, 14, Visibility.Everyone, userName);
            Boolean succeeded = gameEditorService.IsGameLostFilled(userName, gameTitle);
            Assert.False(succeeded);

            // Test with a game lost result, that have no text.
            userName = "TestUser33";
            gameTitle = "IncorrectGameLostResult";
            MakeTestGame(gameTitle, 5, Visibility.LoggedIn, userName);
            GetTestGame(gameTitle, userName).GameLostResult = new GameResult()
            {
                Text = null,
                GameTitle = gameTitle,
                Owner = GetUser(userName),
                IsGameWonResult = false
            };
            _context.SaveChanges();
            succeeded = gameEditorService.IsGameLostFilled(userName, gameTitle);
            Assert.False(succeeded);

            // Test with a game lost result that have empty text.
            userName = "TestUser22";
            gameTitle = "IncorrectGameLostResult2";
            MakeTestGame(gameTitle, 11, Visibility.LoggedIn, userName);
            GetTestGame(gameTitle, userName).GameLostResult = new GameResult()
            {
                Text = "",
                GameTitle = gameTitle,
                Owner = GetUser(userName),
                IsGameWonResult = false
            };
            _context.SaveChanges();
            succeeded = gameEditorService.IsGameLostFilled(userName, gameTitle);
            Assert.False(succeeded);

            // Test with correct game lost result.
            userName = "TestUser22";
            gameTitle = "IncorrectGameLostResult2";
            MakeTestGame(gameTitle, 11, Visibility.LoggedIn, userName);
            GetTestGame(gameTitle, userName).GameLostResult = GenerateGameResult(userName, gameTitle, false, GetLongTestString());
            _context.SaveChanges();
            succeeded = gameEditorService.IsGameLostFilled(userName, gameTitle);
            Assert.True(succeeded);
        }

        [Fact]
        public void TestSetReadyToPlay()
        {
            MakeUniqueTestGames(50, true);
            for (int i = 50; i < 100; ++i)
            {
                String userName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;
                gameEditorService.SetReadyToPlay(userName, gameTitle);
                Assert.True(GetTestGame(gameTitle, userName).IsReadyToPlay);
            }
        }

        [Fact]
        public void TestSetNotReadyToPlay()
        {
            MakeUniqueTestGames(50, true);
            for (int i = 50; i < 100; ++i)
            {
                String userName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;
                gameEditorService.SetNotReadyToPlay(userName, gameTitle);
                Assert.False(GetTestGame(gameTitle, userName).IsReadyToPlay);
            }
        }

        #endregion

        #region Test search for solution

        [Fact]
        public void TestSearchForSolution()
        {
            //===---------- Test on games with map that have solution ---------===//

            // Small map.
            String gameTitle = "SmallMapWithSolution";
            String userName = "TestUser1";
            MakeGameWithCorrectMap(userName, gameTitle, Visibility.Everyone);
            SetStartField(userName, gameTitle, 0, 0);
            SetTargetField(userName, gameTitle, 2, 2);
            int? result = gameEditorService.SearchForSolution(userName, gameTitle);
            Assert.NotNull(result);
            Assert.Equal(4, result);

            // Large map.
            gameTitle = "LargeMapWithSolution";
            userName = "TestUser11";
            MakeGameWithLargeCorrectMap(userName, gameTitle, Visibility.Everyone);
            SetStartField(userName, gameTitle, 14, 14);
            SetTargetField(userName, gameTitle, 0, 0);
            result = gameEditorService.SearchForSolution(userName, gameTitle);
            Assert.NotNull(result);
            Assert.Equal(28, result);

            //===---------- Test on games with map that have no solution ----------=====

            //Small map.
            gameTitle = "SmallMapWithNoSolution";
            userName = "TestUser1";
            MakeGameWithCorrectMap(userName, gameTitle, Visibility.Everyone);
            SetFieldDirections(userName, gameTitle, 1, 1, true, false, false, true);
            SetFieldDirections(userName, gameTitle, 0, 2, false, false, false, true);
            SetFieldDirections(userName, gameTitle, 2, 0, true, false, false, false);
            SetFieldDirections(userName, gameTitle, 2, 1, false, true, false, false);
            SetFieldDirections(userName, gameTitle, 1, 2, false, false, true, false);
            SetStartField(userName, gameTitle, 0, 0);
            SetTargetField(userName, gameTitle, 2, 2);
            result = gameEditorService.SearchForSolution(userName, gameTitle);
            Assert.Null(result);

            // Large map.
            gameTitle = "LargeMapWithNoSolution";
            userName = "TestUser1";
            MakeGameWithLargeCorrectMap(userName, gameTitle, Visibility.Everyone);

            // Edit edges.
            SetFieldDirections(userName, gameTitle, 0, 7, false, true, true, false);
            SetFieldDirections(userName, gameTitle, 0, 6, false, false, true, true);
            SetFieldDirections(userName, gameTitle, 14, 7, true, true, false, false);
            SetFieldDirections(userName, gameTitle, 14, 6, false, false, true, true);

            //Edit the 2 column.
            for (int i = 1; i < 14; ++i)
            {
                SetFieldDirections(userName, gameTitle, i, 6, true, false, true, true);
                SetFieldDirections(userName, gameTitle, i, 7, true, true, true, false);
            }

            SetStartField(userName, gameTitle, 0, 0);
            SetTargetField(userName, gameTitle, 14, 14);
            result = gameEditorService.SearchForSolution(userName, gameTitle);
            Assert.Null(result);
        }

        #endregion

        #region Test usually used getter functions

        [Fact]
        public void TestGetFieldTextContent()
        {
            MakeUniqueTestGames(50, true);
            for (int i = 50; i < 100; ++i)
            {
                String gameTitle = "GameTitle" + i;
                String userName = "TestUser" + (i - 50);
                ICollection<MapRow> map = GetMap(userName, gameTitle);
                foreach (MapRow mapRow in map)
                {
                    foreach (Field field in mapRow.Row)
                    {
                        Assert.Equal(GetFieldText(userName, gameTitle, field.ColNumber, field.RowNumber),
                            gameEditorService.GetFieldTextContent(userName, gameTitle, field.RowNumber, field.ColNumber));
                    }
                }
            }
        }

        [Fact]
        public void TestGetField()
        {
            MakeUniqueTestGames(10, true);
            for (int i = 50; i < 60; ++i)
            {
                String gameTitle = "GameTitle" + i;
                String userName = "TestUser" + (i - 50);
                ICollection<MapRow> map = GetMap(userName, gameTitle);
                foreach (MapRow mapRow in map)
                {
                    foreach (Field field in mapRow.Row)
                    {
                        if (field.ColNumber % 3 == 0 && field.RowNumber % 3 == 0)
                        {
                            field.Trial = GenerateTrial();
                            _context.SaveChanges();
                        }
                        CheckField(gameEditorService.GetField(userName, gameTitle, field.RowNumber, field.ColNumber), userName, gameTitle);
                    }
                }
            }
        }

        #endregion

        //===---------- Test GameplayService's functions ----------===//

        #region Test initialize gameplay
        [Fact]
        public void TestGetGameplayViewModel()
        {
            InitializeTestUsers();
            Random rnd = new Random();
            // Test on a small map.
            String userName = "TestUser1";
            String gameTitle = "GameplayWithSmallMap";
            MakeGameWithCorrectMap(userName, gameTitle, Visibility.Everyone);
            Game game = GetTestGame(gameTitle, userName);
            game.StartField = GetField(gameTitle, userName, rnd.Next(0, 3), rnd.Next(0, 3));
            game.TargetField = GetField(gameTitle, userName, rnd.Next(0, 3), rnd.Next(0, 3));
            _context.SaveChanges();
            CheckInitializedGamePlayViewModel(gameplayService.GetGameplayViewModel(userName, gameTitle), userName, gameTitle);
            CheckInitializedGameplayData(userName, gameTitle);

            // Test on a large map.
            userName = "TestUser2";
            gameTitle = "GameplayWithLargeMap";
            MakeGameWithLargeCorrectMap(userName, gameTitle, Visibility.LoggedIn);
            game = GetTestGame(gameTitle, userName);
            game.StartField = GetField(gameTitle, userName, rnd.Next(0, 15), rnd.Next(0, 15));
            _context.SaveChanges();
            CheckInitializedGamePlayViewModel(gameplayService.GetGameplayViewModel(userName, gameTitle), userName, gameTitle);
            CheckInitializedGameplayData(userName, gameTitle);

            // Test on some random games.
            MakeUniqueTestGames(10, true);
            for (int i = 50; i < 60; ++i)
            {
                gameTitle = "GameTitle" + i;
                userName = "TestUser" + (i - 50);
                CheckInitializedGamePlayViewModel(gameplayService.GetGameplayViewModel(userName, gameTitle), userName, gameTitle);
                CheckInitializedGameplayData(userName, gameTitle);
            }
        }

        private void CheckInitializedGamePlayViewModel(GameplayViewModel model, String userName, String gameTitle)
        {
            Game game = _context.Game.Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                                    .Include(g => g.StartField)
                                    .Include(g => g.TargetField)
                                    .FirstOrDefault();
            Assert.Equal(userName, model.Player.UserName);
            Assert.Equal(gameTitle, model.GameTitle);
            Assert.Equal(game.StartField, model.CurrentPlayerPosition);
            Assert.Equal(game.TargetField, model.TargetField);
            Assert.Equal(0, model.StepCount);
            Assert.False(model.IsGameOver);
            if (game.TableSize <= 5)
            {
                Assert.Equal(3, model.LifeCount);
            }
            else if (game.TableSize <= 10)
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
            GameplayData data = _context.GameplayData.Where(d => d.PlayerName == userName && d.GameTitle == gameTitle)
                                                    .Include(d => d.CurrentPlayerPosition)
                                                    .Include(d => d.VisitedFields)
                                                    .FirstOrDefault();
            Game game = _context.Game.Where(g => g.Title == gameTitle)
                                    .Include(g => g.StartField)
                                    .Include(g => g.TargetField)
                                    .FirstOrDefault();
            Assert.Equal(userName, data.PlayerName);
            Assert.Equal(gameTitle, data.GameTitle);
            Assert.Equal(game.StartField, data.CurrentPlayerPosition);
            Assert.Equal(0, data.StepCount);
            Assert.Equal(GameCondition.OnGoing, data.GameCondition);
            if (game.TableSize <= 5)
            {
                Assert.Equal(3, data.LifeCount);
            }
            else if (game.TableSize <= 10)
            {
                Assert.Equal(5, data.LifeCount);
            }
            else
            {
                Assert.Equal(7, data.LifeCount);
            }
            foreach (IsVisitedField field in data.VisitedFields)
            {
                Assert.False(field.IsVisited);
            }

        }
        #endregion

        #region Step and load game

        [Fact]
        public void TestStepPlayCounter()
        {
            MakeUniqueTestGames(30, true);
            for (int i = 50; i < 80; ++i)
            {
                String gameTitle = "GameTitle" + i;
                gameplayService.StepPlayCounter(gameTitle);
                Assert.Equal(1, GetGameByTitle(gameTitle).PlayCounter);
                gameplayService.StepPlayCounter(gameTitle);
                Assert.Equal(2, GetGameByTitle(gameTitle).PlayCounter);
            }
        }

        [Fact]
        public void TestStepGame()
        {
            #region Test on a game with small map.

            String userName = "TestUser44";
            String playerName = "TestUser0";
            String gameTitle = "TestStepGameOnSmallMap";
            MakeGameWithCorrectMap(userName, gameTitle, Visibility.Everyone);

            // Set start and target fields.
            Game game = _context.Game.Where(g => g.Title == gameTitle && g.Owner.UserName == userName).FirstOrDefault();
            game.StartField = GetField(gameTitle, userName, 0, 0);
            game.TargetField = GetField(gameTitle, userName, 2, 2);
            game.TargetField.Trial = null;
            _context.SaveChanges();
            InitializeGameplayData(gameTitle, playerName);

            // Check stepping player in every direction on the map.
            CheckStepGameInEveryDirection(playerName, userName, gameTitle);

            // Check what happens if step in to invalid directions (there's no way to that direction and it goes out 
            //from them map.
            Field newPlayerPosition = gameplayService.StepGame(playerName, gameTitle, Direction.Left);
            // It should leave the player on the field that he/she is on.
            Assert.Equal(GetField(gameTitle, userName, 0, 0), newPlayerPosition);
            Assert.Equal(GameCondition.OnGoing, GetGameplayData(playerName, gameTitle).GameCondition);

            newPlayerPosition = gameplayService.StepGame(playerName, gameTitle, Direction.Up);
            Assert.Equal(GetField(gameTitle, userName, 0, 0), newPlayerPosition);
            Assert.Equal(GameCondition.OnGoing, GetGameplayData(playerName, gameTitle).GameCondition);

            // Step player to the target field, then check if game is over when we reach the target field (2,2).
            newPlayerPosition = gameplayService.StepGame(playerName, gameTitle, Direction.Down);
            Assert.Equal(GetField(gameTitle, userName, 1, 0), newPlayerPosition);
            Assert.True(GetIsVisitedField(gameTitle, playerName, 0, 0).IsVisited);
            Assert.Equal(GameCondition.OnGoing, GetGameplayData(playerName, gameTitle).GameCondition);

            newPlayerPosition = gameplayService.StepGame(playerName, gameTitle, Direction.Down);
            Assert.Equal(GetField(gameTitle, userName, 2, 0), newPlayerPosition);
            Assert.True(GetIsVisitedField(gameTitle, playerName, 0, 1).IsVisited);
            Assert.Equal(GameCondition.OnGoing, GetGameplayData(playerName, gameTitle).GameCondition);

            newPlayerPosition = gameplayService.StepGame(playerName, gameTitle, Direction.Right);
            Assert.Equal(GetField(gameTitle, userName, 2, 1), newPlayerPosition);
            Assert.True(GetIsVisitedField(gameTitle, playerName, 0, 2).IsVisited);
            Assert.Equal(GameCondition.OnGoing, GetGameplayData(playerName, gameTitle).GameCondition);

            newPlayerPosition = gameplayService.StepGame(playerName, gameTitle, Direction.Right);
            Assert.Equal(GetField(gameTitle, userName, 2, 2), newPlayerPosition);
            Assert.True(GetIsVisitedField(gameTitle, playerName, 1, 2).IsVisited);
            Assert.Equal(GameCondition.Won, GetGameplayData(playerName, gameTitle).GameCondition);
            #endregion

            #region Test on a game with large map.

            userName = "TestUser5";
            playerName = "TesUser49";
            gameTitle = "TestStepGameOnLargeMap";
            MakeGameWithLargeCorrectMap(userName, gameTitle, Visibility.Everyone);

            // Set start and target fields.
            game = _context.Game.Where(g => g.Title == gameTitle && g.Owner.UserName == userName).FirstOrDefault();
            game.StartField = GetField(gameTitle, userName, 0, 0);
            game.TargetField = GetField(gameTitle, userName, 14, 14);
            game.TargetField.Trial = null;
            _context.SaveChanges();
            InitializeGameplayData(gameTitle, playerName);

            // Check stepping player in every direction on the map.
            CheckStepGameInEveryDirection(playerName, userName, gameTitle);

            // Check what happens if we step the player to the target field (14, 14).

            Field lastPlayerPosition = _context.GameplayData.Where(d => d.GameTitle == gameTitle && d.PlayerName == playerName)
                                                            .Include(d => d.CurrentPlayerPosition)
                                                            .Select(d => d.CurrentPlayerPosition)
                                                            .FirstOrDefault();
            // Step in direction down.
            for (int i = 0; i < 14; ++i)
            {
                newPlayerPosition = gameplayService.StepGame(playerName, gameTitle, Direction.Down);
                Assert.Equal(GetField(gameTitle, userName, lastPlayerPosition.RowNumber + 1, lastPlayerPosition.ColNumber),
                    newPlayerPosition);
                Assert.True(GetIsVisitedField(gameTitle, playerName, lastPlayerPosition.ColNumber, lastPlayerPosition.RowNumber)
                    .IsVisited);
                Assert.Equal(GameCondition.OnGoing, GetGameplayData(playerName, gameTitle).GameCondition);
                lastPlayerPosition = newPlayerPosition;
            }
            // Step in direction right.

            for (int i = 0; i < 14; ++i)
            {
                newPlayerPosition = gameplayService.StepGame(playerName, gameTitle, Direction.Right);
                Assert.Equal(GetField(gameTitle, userName, lastPlayerPosition.RowNumber, lastPlayerPosition.ColNumber + 1),
                    newPlayerPosition);
                Assert.True(GetIsVisitedField(gameTitle, playerName, lastPlayerPosition.ColNumber, lastPlayerPosition.RowNumber)
                    .IsVisited);
                lastPlayerPosition = newPlayerPosition;
            }
            Assert.Equal(GameCondition.Won, GetGameplayData(playerName, gameTitle).GameCondition);

            #endregion
        }

        [Fact]
        public void TestGameplayGetGameResult()
        {
            MakeUniqueTestGames(20, true);
            for (int i = 50; i < 70; ++i)
            {
                String gameTitle = "GameTitle" + i;
                String ownerName = "TestUser" + (i - 50);
                String playerName = "TestUser" + Math.Abs(i - 60);

                // Check on GameWonResult.
                InitializeGameplayData(gameTitle, playerName);
                GameplayData gameplayData = GetGameplayData(playerName, gameTitle);
                Game game = _context.Game.Where(g => g.Title == gameTitle && g.Owner.UserName == ownerName)
                                         .Include(g => g.GameWonResult)
                                         .Include(g => g.GameLostResult)
                                         .FirstOrDefault();
                gameplayData.GameCondition = GameCondition.Won;
                _context.SaveChanges();
                Assert.Equal(game.GameWonResult, gameplayService.GetGameResult(gameTitle, playerName));
                Assert.Null(GetGameplayData(playerName, gameTitle));

                // Check on GameLostResult
                InitializeGameplayData(gameTitle, playerName);
                gameplayData = GetGameplayData(playerName, gameTitle);
                game = _context.Game.Where(g => g.Title == gameTitle && g.Owner.UserName == ownerName)
                                         .Include(g => g.GameWonResult)
                                         .Include(g => g.GameLostResult)
                                         .FirstOrDefault();
                gameplayData.GameCondition = GameCondition.Lost;
                _context.SaveChanges();
                Assert.Equal(game.GameLostResult, gameplayService.GetGameResult(gameTitle, playerName));
                Assert.Null(GetGameplayData(playerName, gameTitle));
            }
        }

        [Fact]
        public void TestSetGameOver()
        {
            MakeUniqueTestGames(20, true);
            for (int i = 50; i < 70; i++)
            {
                String ownerName = "TestUser" + i;
                String gameTitle = "GameTitle" + i;
                String playerName = "TestUser" + Math.Abs(i - 60);
                InitializeGameplayData(gameTitle, playerName);

                // Test set game won.
                gameplayService.SetGameOver(playerName, gameTitle, true);
                Assert.Equal(GameCondition.Won, GetGameplayData(playerName, gameTitle).GameCondition);

                // Test set game lost.
                gameplayService.SetGameOver(playerName, gameTitle, false);
                Assert.Equal(GameCondition.Lost, GetGameplayData(playerName, gameTitle).GameCondition);
            }
        }

        #endregion

        #region Test doing trial

        [Fact]
        public void TestGetDirectionButtonsAfterTrial()
        {
            MakeUniqueTestGames(5, false);
            for (int i = 50; i < 55; ++i)
            {
                String ownerName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;
                String playerName = "TestUser" + Math.Abs(i - 60);

                // Set trials.
                foreach (MapRow mapRow in GetMap(ownerName, gameTitle))
                {
                    foreach (Field field in mapRow.Row)
                    {
                        if (i % 2 == 0)
                        {
                            field.Trial = GenerateTrialForTesting(true);
                        }
                        else
                        {
                            field.Trial = GenerateTrialForTesting(false);
                        }
                    }
                }
                _context.SaveChanges();
                InitializeGameplayData(gameTitle, playerName);

                // Test with each field's trial.
                foreach (MapRow mapRow in GetMap(ownerName, gameTitle))
                {
                    foreach (Field field in mapRow.Row)
                    {
                        for (int j = 0; j < 4; ++j)
                        {
                            GameplayData gameplayData = GetGameplayData(playerName, gameTitle);
                            int lifeCountBefore = gameplayData.LifeCount;
                            DirectionButtonsViewModel model =
                            gameplayService.GetDirectionButtonsAfterTrial(playerName, gameTitle, field.ColNumber, field.RowNumber, j, false);

                            // Check that values of the model that aren't in connection of the trial's result.
                            Assert.Equal(gameTitle, model.GameTitle);
                            Assert.Equal(field.RowNumber, model.RowNumber);
                            Assert.Equal(field.ColNumber, model.ColNumber);
                            Assert.Equal(field.IsUpWay, model.IsUpWay);
                            Assert.Equal(field.IsDownWay, model.IsDownWay);
                            Assert.Equal(field.IsLeftWay, model.IsLeftWay);
                            Assert.Equal(field.IsRightWay, model.IsRightWay);

                            gameplayData = GetGameplayData(playerName, gameTitle);
                            if (i % 2 == 0)
                            {
                                switch (j)
                                {
                                    // ResultType.GameWon
                                    case 0:
                                        Assert.True(model.GameWon);
                                        Assert.Equal(GameCondition.Won, gameplayData.GameCondition);
                                        break;
                                    //ResultType.GetLife
                                    case 1:
                                        Assert.Equal(lifeCountBefore + 1, gameplayData.LifeCount);
                                        break;
                                    //ResultType.GetTargetDirection
                                    case 2:
                                        // In this case, there's no special value to check.
                                        break;
                                    //ResultType.LoseLife
                                    default:
                                        if (lifeCountBefore > 0)
                                        {
                                            Assert.Equal(lifeCountBefore - 1, gameplayData.LifeCount);
                                        }
                                        if (lifeCountBefore <= 1)
                                        {
                                            Assert.True(model.GameLost);
                                            Assert.Equal(GameCondition.Lost, gameplayData.GameCondition);
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                switch (j)
                                {
                                    // ResultType.GetTargetDirection
                                    case 0:
                                        // In this case, there's no special value to check.
                                        break;
                                    // ResultType.LoseLife
                                    case 1:
                                        if (lifeCountBefore > 0)
                                        {
                                            Assert.Equal(lifeCountBefore - 1, gameplayData.LifeCount);
                                        }
                                        if (lifeCountBefore <= 1)
                                        {
                                            Assert.True(model.GameLost);
                                            Assert.Equal(GameCondition.Lost, gameplayData.GameCondition);
                                        }
                                        break;
                                    // ResultType.Nothing
                                    case 2:
                                        // In this case, there's no special value to check.
                                        break;
                                    // ResultType.Teleport
                                    default:
                                        Assert.True(model.WillTeleport);
                                        break;
                                }
                            }
                        }

                    }
                }
            }
        }

        [Fact]
        public void TestGetTargetDirection()
        {
            //===----- Test on a game with small map. -----===//

            String userName = "TestUser24";
            String gameTitle = "TestGetTargetDirectionOnSmallMap";
            MakeGameWithCorrectMap(userName, gameTitle, Visibility.Everyone);
            Game game = _context.Game.Where(g => g.Title == gameTitle && g.Owner.UserName == userName).FirstOrDefault();
            // Set target and start direction
            game.StartField = GetField(gameTitle, userName, 0, 1);
            game.TargetField = GetField(gameTitle, userName, 1, 1);

            // Test on direction North.
            Assert.Equal(CompassPoint.North, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 2, 1)));

            // Test on direction South.
            Assert.Equal(CompassPoint.South, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 0, 1)));

            // Test on direction East.
            Assert.Equal(CompassPoint.East, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 1, 0)));

            // Test on direction West.
            Assert.Equal(CompassPoint.West, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 1, 2)));

            //Test on direction Nort-East.
            Assert.Equal(CompassPoint.NorthEast, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 2, 0)));

            // Test on direction Nort-West.
            Assert.Equal(CompassPoint.NorthWest, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 2, 2)));

            // Test on direction South-East.
            Assert.Equal(CompassPoint.SouthEast, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 0, 0)));

            // Test on direction South-West.
            Assert.Equal(CompassPoint.SouthWest, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 0, 2)));

            // Check if the field is the target field.
            Assert.Equal(CompassPoint.Here, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 1, 1)));


            //===----- Test on a game with large map -----===//

            userName = "TestUser42";
            gameTitle = "TestGetTargetDirectionOnLargeMap";
            MakeGameWithLargeCorrectMap(userName, gameTitle, Visibility.Everyone);
            game = _context.Game.Where(g => g.Title == gameTitle && g.Owner.UserName == userName).FirstOrDefault();
            // Set target and start direction
            game.StartField = GetField(gameTitle, userName, 0, 1);
            game.TargetField = GetField(gameTitle, userName, 7, 7);

            // Test on direction North.
            Assert.Equal(CompassPoint.North, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 14, 7)));
            Assert.Equal(CompassPoint.North, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 10, 7)));

            // Test on direction South.
            Assert.Equal(CompassPoint.South, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 0, 7)));
            Assert.Equal(CompassPoint.South, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 3, 7)));

            // Test on direction East.
            Assert.Equal(CompassPoint.East, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 7, 0)));
            Assert.Equal(CompassPoint.East, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 7, 2)));

            // Test on direction West.
            Assert.Equal(CompassPoint.West, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 7, 14)));
            Assert.Equal(CompassPoint.West, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 7, 8)));

            //Test on direction Nort-East.
            Assert.Equal(CompassPoint.NorthEast, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 14, 0)));
            Assert.Equal(CompassPoint.NorthEast, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 10, 0)));

            // Test on direction Nort-West.
            Assert.Equal(CompassPoint.NorthWest, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 14, 14)));
            Assert.Equal(CompassPoint.NorthWest, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 13, 8)));

            // Test on direction South-East.
            Assert.Equal(CompassPoint.SouthEast, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 0, 0)));
            Assert.Equal(CompassPoint.SouthEast, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 2, 5)));

            // Test on direction South-West.
            Assert.Equal(CompassPoint.SouthWest, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 0, 14)));
            Assert.Equal(CompassPoint.SouthWest, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 3, 10)));

            // Check if the field is the target field.
            Assert.Equal(CompassPoint.Here, gameplayService.GetTargetDirection(gameTitle, GetField(gameTitle, userName, 7, 7)));
        }

        #endregion

        #region Test teleport

        [Fact]
        public void TestGetRandDirection()
        {
            for (int i = 0; i < 50; ++i)
            {
                Assert.IsType<Direction>(gameplayService.GetRandDirection());
                Assert.True(gameplayService.GetRandDirection() != Direction.NotSet);
            }
        }

        [Fact]
        public void TestGetGameMapSize()
        {
            MakeDefaultTestGames();
            for (int i = 0; i < 50; ++i)
            {
                String gameTitle = "GameTitle" + i;
                Assert.Equal(_context.Game.Where(g => g.Title == gameTitle).FirstOrDefault().TableSize,
                    gameplayService.GetGameMapSize(gameTitle));
            }
        }

        #endregion

        #region Test usually used getters

        [Fact]
        public void TestGetGameplayFieldViewModel()
        {
            MakeUniqueTestGames(20, true);
            for (int i = 50; i < 70; ++i)
            {
                String ownerName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;
                String playerName = "TestUser" + Math.Abs(i - 60);
                InitializeGameplayData(gameTitle, playerName);
                foreach (MapRow mapRow in GetMap(ownerName, gameTitle))
                {
                    foreach (Field field in mapRow.Row)
                    {
                        GameplayFieldViewModel model = gameplayService.GetGameplayFieldViewModel(playerName, gameTitle, field);
                        Assert.Equal(gameTitle, model.GameTitle);
                        Assert.Equal(field.RowNumber, model.RowNumber);
                        Assert.Equal(field.ColNumber, model.ColNumber);
                        Assert.Equal(field.Text, model.Text);
                        Assert.Equal(field.Trial, model.Trial);
                        Assert.Equal(field.IsRightWay, model.IsRightWay);
                        Assert.Equal(field.IsLeftWay, model.IsLeftWay);
                        Assert.Equal(field.IsUpWay, model.IsUpWay);
                        Assert.Equal(field.IsDownWay, model.IsDownWay);
                        Assert.Equal(GetIsVisitedField(gameTitle, playerName, field.ColNumber, field.RowNumber).IsVisited,
                            model.IsVisited);
                        Field targetField = _context.Game
                            .Where(g => g.Title == gameTitle)
                            .Include(g => g.TargetField)
                            .Select(g => g.TargetField)
                            .FirstOrDefault();
                        Assert.Equal((targetField.ColNumber == field.ColNumber && targetField.RowNumber == field.RowNumber), model.IsAtTargetField);
                        Assert.Equal(GetGameplayData(playerName, gameTitle).LifeCount, model.LifeCount);
                    }
                }
            }
        }

        [Fact]
        public void TestGetIsVisitedField()
        {
            MakeUniqueTestGames(10, true);

            for (int i = 50; i < 60; ++i)
            {
                String gameTitle = "GameTitle" + i;
                String ownerName = "TestUser" + (i - 50);
                String playerName = "TestUser" + Math.Abs(i - 60);
                InitializeGameplayData(gameTitle, playerName);
                foreach (IsVisitedField field in
                    _context.GameplayData
                    .Where(d => d.GameTitle == gameTitle && d.PlayerName == playerName)
                    .Include(d => d.VisitedFields)
                    .Select(d => d.VisitedFields)
                    .FirstOrDefault()
                    .ToList())
                {
                    if (field.ColNumber % 2 == 0)
                    {
                        field.IsVisited = true;
                        _context.SaveChanges();
                        Assert.True(gameplayService.GetIsVisitedField(playerName, gameTitle, field.ColNumber, field.RowNumber));
                    }
                    Assert.Equal(GetIsVisitedField(gameTitle, playerName, field.ColNumber, field.RowNumber).IsVisited,
                        gameplayService.GetIsVisitedField(playerName, gameTitle, field.ColNumber, field.RowNumber));
                }
            }
        }

        [Fact]
        public void TestGameplayGetTrial()
        {
            MakeUniqueTestGames(30, true);
            for(int i = 50; i < 80; ++i)
            {
                String userName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;
                foreach(MapRow mapRow in GetMap(userName, gameTitle))
                {
                    foreach(Field field in mapRow.Row)
                    {
                        if(field.Trial != null){
                            Trial trial = gameplayService.GetTrial(gameTitle, field.ColNumber, field.RowNumber);
                            CheckTrial(trial.Alternatives, trial.Text, field);
                        }                        
                    }
                }
            }
        }

        [Fact]
        public void TestIsAtTargetField()
        {
            MakeUniqueTestGames(30, true);
            for(int i = 50; i < 80; ++i)
            {
                String gameTitle = "GameTitle" + i;
                String userName = "TestUser" + (i - 50);
                Field targetField = _context.Game.Where(g => g.Title == gameTitle)
                                                 .Include(g => g.TargetField)
                                                 .Select(g => g.TargetField)
                                                 .FirstOrDefault();
                foreach(MapRow mapRow in GetMap(userName, gameTitle))
                {
                    foreach(Field field in mapRow.Row)
                    {
                        if(field.ColNumber == targetField.ColNumber && field.RowNumber == targetField.RowNumber)
                        {
                            Assert.True(gameplayService.IsAtTargetField(gameTitle, field.RowNumber, field.ColNumber));
                        }
                        else
                        {
                            Assert.False(gameplayService.IsAtTargetField(gameTitle, field.RowNumber, field.ColNumber));
                        }
                    }
                }
            }
        }

        [Fact]
        public void TestGameplayGetField()
        {
            MakeUniqueTestGames(20, true);
            for(int i = 50; i < 70; ++i)
            {
                String userName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;
                foreach(MapRow mapRow in GetMap(userName, gameTitle))
                {
                    foreach(Field field in mapRow.Row)
                    {
                        CheckField(gameplayService.GetField(gameTitle, field.RowNumber, field.ColNumber), userName, gameTitle);
                    }
                }
            }
        }

        [Fact]
        public void TestGetLifeCount()
        {
            MakeUniqueTestGames(20, true);
            for(int i = 50; i < 70; ++i)
            {
                String userName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;
                String playerName = "TestUser" + Math.Abs(i - 60);
                InitializeGameplayData(gameTitle, playerName);
                int mapSize = _context.Game.Where(g => g.Title == gameTitle).Select(g => g.TableSize).FirstOrDefault();
                int lifeCount = 3;
                if (mapSize > 5) lifeCount = 5;
                if (mapSize > 10) lifeCount = 7;
                Assert.Equal(lifeCount, gameplayService.GetLifeCount(playerName, gameTitle));
                _context.GameplayData
                    .Where(d => d.GameTitle == gameTitle && d.PlayerName == playerName)
                    .FirstOrDefault()
                    .LifeCount = lifeCount + 1;
                _context.SaveChanges();
                Assert.Equal(lifeCount+1, gameplayService.GetLifeCount(playerName, gameTitle));
                _context.GameplayData
                    .Where(d => d.GameTitle == gameTitle && d.PlayerName == playerName)
                    .FirstOrDefault()
                    .LifeCount = lifeCount - 2;
                _context.SaveChanges();
                Assert.Equal(lifeCount - 2, gameplayService.GetLifeCount(playerName, gameTitle));
            }
        }

        #endregion

        #region Helper functions for testing GameplayService

        #region Generate test data
        private void InitializeGameplayData(String gameTitle, String playerName)
        {
            Game game = _context.Game.Where(g => g.Title == gameTitle)
                .Include(g => g.StartField)
                .FirstOrDefault();
            int lifeCount = 3;
            if (game.TableSize > 5) lifeCount = 5;
            if (game.TableSize > 10) lifeCount = 7;
            GameplayData gameplayData = new GameplayData()
            {
                PlayerName = playerName,
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
            _context.SaveChanges();
        }

        private List<IsVisitedField> InitializeVisitedFields(int mapSize)
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

        // Generate test trials with the first 4 ResultTy of the 6 if the attribute is true, and with the second 4 ResultType of the 6 if 
        // false.
        private Trial GenerateTrialForTesting(Boolean isFirstSectionOfResultTypes)
        {
            List<Alternative> alternatives = new List<Alternative>();
            for(int i = 0; i < 4; ++i)
            {
                TrialResult trialResult = new TrialResult()
                {
                    Text = GetLongTestString()
                };
                if (isFirstSectionOfResultTypes)
                {
                    switch (i)
                    {
                        case 0:
                            trialResult.ResultType = ResultType.GameWon;
                            break;
                        case 1:
                            trialResult.ResultType = ResultType.GetLife;
                            break;
                        case 2:
                            trialResult.ResultType = ResultType.GetTargetDirection;
                            break;
                        default:
                            trialResult.ResultType = ResultType.LoseLife;
                            break;
                    }
                }
                else
                {
                    switch (i)
                    {
                        case 0:
                            trialResult.ResultType = ResultType.GetTargetDirection;
                            break;
                        case 1:
                            trialResult.ResultType = ResultType.LoseLife;
                            break;
                        case 2:
                            trialResult.ResultType = ResultType.Nothing;
                            break;
                        default:
                            trialResult.ResultType = ResultType.Teleport;
                            break;
                    }
                }
                
                alternatives.Add(new Alternative()
                {
                    Text = GetLongTestString(),
                    TrialResult = trialResult
                });
            }
            Trial trial = new Trial()
            {
                Text = GetLongTestString(),
                Alternatives = alternatives
            };
            return trial;
        }

        #endregion

        #region Getters

        private GameplayData GetGameplayData(String playerName, String gameTitle)
        {
            return _context.GameplayData.Where(d => d.GameTitle == gameTitle && d.PlayerName == playerName).FirstOrDefault();
        }

        private Game GetGameByTitle(String gameTitle)
        {
            return _context.Game.Where(g => g.Title == gameTitle).FirstOrDefault();
        }

        private IsVisitedField GetIsVisitedField(String gameTitle, String playerName, int colNumber, int rowNumber)
        {
            return _context.GameplayData
                .Where(d => d.GameTitle == gameTitle && d.PlayerName == playerName)
                .Include(d => d.VisitedFields)
                .Select(d => d.VisitedFields)
                .FirstOrDefault()
                .Where(f => f.ColNumber == colNumber && f.RowNumber == rowNumber)
                .FirstOrDefault();
        }

        #endregion

        #region Check test data

        // To use this, we have to provide a map that we can reach a field in direction right and down from the start field.
        private void CheckStepGameInEveryDirection(String playerName, String ownerName, String gameTitle)
        {
            Game game = _context.Game.Where(g => g.Title == gameTitle && g.Owner.UserName == ownerName).FirstOrDefault();

            // Check step in direction down.
            Field newPlayerPosition = gameplayService.StepGame(playerName, gameTitle, Direction.Down);
            Assert.Equal(GetField(gameTitle, ownerName, 1, 0), newPlayerPosition);
            Assert.True(GetIsVisitedField(gameTitle, playerName, game.StartField.ColNumber, game.StartField.RowNumber).IsVisited);
            Assert.Equal(GameCondition.OnGoing, GetGameplayData(playerName, gameTitle).GameCondition);

            //Check step in direction right.
            newPlayerPosition = gameplayService.StepGame(playerName, gameTitle, Direction.Right);
            Assert.Equal(GetField(gameTitle, ownerName, 1, 1), newPlayerPosition);
            Assert.True(GetIsVisitedField(gameTitle, playerName, 0, 1).IsVisited);
            Assert.Equal(GameCondition.OnGoing, GetGameplayData(playerName, gameTitle).GameCondition);

            // Check step in direction up.
            newPlayerPosition = gameplayService.StepGame(playerName, gameTitle, Direction.Up);
            Assert.Equal(GetField(gameTitle, ownerName, 0, 1), newPlayerPosition);
            Assert.True(GetIsVisitedField(gameTitle, playerName, 1, 1).IsVisited);
            Assert.Equal(GameCondition.OnGoing, GetGameplayData(playerName, gameTitle).GameCondition);

            // Check step in direction left.
            newPlayerPosition = gameplayService.StepGame(playerName, gameTitle, Direction.Left);
            Assert.Equal(GetField(gameTitle, ownerName, 0, 0), newPlayerPosition);
            Assert.True(GetIsVisitedField(gameTitle, playerName, 1, 0).IsVisited);
            Assert.Equal(GameCondition.OnGoing, GetGameplayData(playerName, gameTitle).GameCondition);
        }

        #endregion

        #endregion


        #region Helper functions

        #region Generate data for testing

        private void InitializeTestUsers()
        {
            List<User> users = new List<User>();
            for(int i = 0; i < 50; ++i)
            {
                if(!_context.User.Any(u => u.UserName == "TestUser" + i))
                {
                    users.Add(new User()
                    {
                        UserName = "TestUser" + i,
                        NickName = "TestUserNick" + i,
                        Email = "testuser" + i + "@gmail.com"
                    });
                }
            }
            _context.User.AddRange(users);
            _context.SaveChanges();
        }

        private void InitializeGame(String userName, String gameTitle, int mapSize,  Visibility visibility)
        {
            InitializeTestUsers();
            Game game = _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle).FirstOrDefault();
            if (game != null){
                _context.Game.Remove(game);
                _context.SaveChanges();
            }

            // Initialize a map.            
            List<MapRow> map = new List<MapRow>();
            for (int i = 0; i < mapSize; ++i)
            {
                MapRow row = new MapRow
                {
                    Row = new List<Field>()
                };
                for (int j = 0; j < mapSize; ++j)
                {

                    row.Row.Add(
                        new Field
                        {
                            UserName = userName,
                            GameTitle = gameTitle,
                            RowNumber = i,
                            ColNumber = j,
                            Text = "",
                            IsRightWay = false,
                            IsLeftWay = false,
                            IsUpWay = false,
                            IsDownWay = false
                        });
                }
                map.Add(row);
            }

            // Save the initialized game to the database.
            _context.Game.Add(
                new Game
                {
                    Title = gameTitle,
                    Visibility = visibility,
                    TableSize = mapSize,
                    PlayCounter = 0,
                    Owner = GetUser(userName),
                    Map = map,
                    CurrentWayDirectionsCode = 0,
                    IsReadyToPlay = false
                }
                );
            _context.SaveChanges();
        }

        private void MakeGameWithCorrectMap(String userName, String gameTitle, Visibility visibility)
        {
            InitializeTestUsers();
            MakeTestGame(gameTitle, 3, visibility, userName);
            EditDefaultMap(GetMap(userName, gameTitle), true);
            SetFieldDirections(userName, gameTitle, 0, 0, false, true, true, false);
            SetFieldDirections(userName, gameTitle, 0, 1, false, true, true, true);
            SetFieldDirections(userName, gameTitle, 0, 2, false, false, true, true);

            SetFieldDirections(userName, gameTitle, 1, 0, true, true, true, false);
            SetFieldDirections(userName, gameTitle, 1, 1, true, true, true, true);
            SetFieldDirections(userName, gameTitle, 1, 2, true, false, true, true);

            SetFieldDirections(userName, gameTitle, 2, 0, true, true, false, false);
            SetFieldDirections(userName, gameTitle, 2, 1, true, true, false, true);
            SetFieldDirections(userName, gameTitle, 2, 2, true, false, false, true);
        }

        private void MakeGameWithLargeCorrectMap(String userName, String gameTitle, Visibility visibility)
        {
            MakeTestGame(gameTitle, 15, visibility, userName);

            // Set Corners.
            SetFieldDirections(userName, gameTitle, 0, 0, false, true, true, false);
            SetFieldDirections(userName, gameTitle, 0, 14, false, false, true, true);
            SetFieldDirections(userName, gameTitle, 14, 0, true, true, false, false);
            SetFieldDirections(userName, gameTitle, 14, 14, true, false, false, true);

            // Set edges.
            for(int i = 1; i < 14; ++i)
            {
                SetFieldDirections(userName, gameTitle, 0, i, false, true, true, true);
                SetFieldDirections(userName, gameTitle, 14, i, true, true, false, true);
                SetFieldDirections(userName, gameTitle, i, 0, true, true, true, false);
                SetFieldDirections(userName, gameTitle, i, 14, true, false, true, true);
            }

            // Set fields inside the map.
            for(int i = 1; i < 14; ++i)
            {
                for (int j = 1; j < 14; ++j)
                {
                    SetFieldDirections(userName, gameTitle, i, j, true, true, true, true);
                }
            }
        }

        private GameResult GenerateGameResult(String userName, String gameTitle, Boolean isGameWonResult, String text)
        {
            return new GameResult()
            {
                Owner = _context.User.Where(u => u.UserName == userName).FirstOrDefault(),
                GameTitle = gameTitle,
                IsGameWonResult = isGameWonResult,
                Text = text
            };
        }

        private Prelude GeneratePrelude(String userName, String gameTitle, String text)
        {
            return new Prelude()
            {
                Text = text,
                GameTitle = gameTitle,
                Owner = _context.User.Where(u => u.UserName == userName).FirstOrDefault()
            };
        }

        private void MakeDefaultTestGames()
        {
            InitializeTestUsers();
            for (int i = 0; i < 50; ++i)
            {
                InitializeGame("TestUser" + i, "GameTitle" + i, (i % 12) + 3, Visibility.Everyone);
            }
        }

        private void MakeUniqueTestGames(int gameCount, Boolean isAddAllContent)
        {
            InitializeTestUsers();
            for (int i = 50; i < 50 + gameCount; ++i)
            {
                String userName = "TestUser" + (i - 50);
                String gameTitle = "GameTitle" + i;
                if (!_context.Game.Any(g => g.Owner.UserName == userName && g.Title == gameTitle))
                {
                    Random rnd = new Random();
                    int random = rnd.Next(1, 4);
                    Visibility randomVisibility = Visibility.Everyone;
                    switch (random) {
                        case 1:
                            randomVisibility = Visibility.Everyone;
                            break;
                        case 2:
                            randomVisibility = Visibility.LoggedIn;
                            break;
                        default:
                            randomVisibility = Visibility.Owner;
                            break;
                    }
                    int mapSize = rnd.Next(3, 16);
                    InitializeGame(userName, gameTitle, mapSize, randomVisibility);
                    ICollection<MapRow> map = _context.Game
                                                  .Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                                                  .Select(g => g.Map)
                                                  .FirstOrDefault();
                    map = EditDefaultMap(map, isAddAllContent);
                    Game game = GetTestGame(gameTitle, userName);
                    game.StartField = GetField(gameTitle, userName, rnd.Next(0, mapSize), rnd.Next(0, mapSize));
                    game.TargetField = GetField(gameTitle, userName, rnd.Next(0, mapSize), rnd.Next(0, mapSize));
                    game.Summary = "Test summary text.";
                    game.GameLostResult = new GameResult()
                    {
                        Owner = _context.User.Where(u => u.UserName == userName).FirstOrDefault(),
                        GameTitle = gameTitle,
                        IsGameWonResult = false,
                        Text = "Test game lost result text."
                    };
                    game.GameWonResult = new GameResult()
                    {
                        Owner = _context.User.Where(u => u.UserName == userName).FirstOrDefault(),
                        GameTitle = gameTitle,
                        IsGameWonResult = true,
                        Text = "Test game won result text."
                    };
                    game.Prelude = new Prelude()
                    {
                        Text = "Tet prelude text.",
                        GameTitle = gameTitle,
                        Owner = _context.User.Where(u => u.UserName == userName).FirstOrDefault()
                    };
                    _context.SaveChanges();

                }
            }
        }

        private void MakeTestGame(String gameTitle, int tableSize, Visibility visibility, String userName)
        {
            InitializeTestUsers();
            InitializeGame(userName, gameTitle, tableSize, visibility);

            ICollection<MapRow> map = _context.Game
                                              .Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                                              .Select(g => g.Map)
                                              .FirstOrDefault();
            map = EditDefaultMap(map, false);
            _context.SaveChanges();
        }

        // Edits the map's ways randomly. (These are empty by default.)
        private ICollection<MapRow> EditDefaultMap(ICollection<MapRow> map, Boolean isAddAllContents)
        {
            Random rnd = new Random();
            int random = 0;

            foreach (MapRow mapRow in map)
            {
                foreach (Field field in mapRow.Row)
                {
                    // Edit ways randomly.
                    random = rnd.Next(0, 2); // creates a number between 0 and 1
                    if (random == 0)
                    {
                        field.IsDownWay = true;
                    }
                    random = rnd.Next(0, 2);
                    if (random == 0)
                    {
                        field.IsUpWay = true;
                    }
                    random = rnd.Next(0, 2);
                    if (random == 0)
                    {
                        field.IsLeftWay = true;
                    }
                    random = rnd.Next(0, 2);
                    if (random == 0)
                    {
                        field.IsRightWay = true;
                    }

                    // Adding more data randomly.
                    random = rnd.Next(0, 2);
                    if (random == 0 || isAddAllContents)
                    {
                        if (field.ColNumber == 0)
                        {
                            field.Text = ""; // Be sure testing it on an empty string.
                        }
                        else if (field.ColNumber == 1)
                        {
                            // Be sure to test it on a very long text.
                            field.Text = "it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.it's a very long text.";
                        }
                        else
                        {
                            random = rnd.Next(0, 100);
                            field.Text = "test text" + random;
                        }

                    }

                    random = rnd.Next(0, 2);
                    if (random == 0 || isAddAllContents)
                    {
                        random = rnd.Next(0, 100);
                        field.Trial = new Trial
                        {
                            Text = "the test trial's text" + random,
                            Alternatives = GenerateAlternatives()
                        };
                    }

                }
            }
            return map;
        }

        private Trial GenerateTrial()
        {
            Random rnd = new Random();
            return new Trial()
            {
                Text = "Test trial text. Test trial text. Test trial text. Test trial text." + rnd.Next(1, 10000),
                Alternatives = GenerateAlternatives()
            };
        }

        private List<Alternative> GenerateAlternatives()
        {
            List<Alternative> alternatives = new List<Alternative>();
            for (int i = 0; i < 4; ++i)
            {
                Random rnd = new Random();
                int random = rnd.Next(0, 5);
                ResultType randomResult = ResultType.Nothing;
                switch (random)
                {
                    case 0:
                        randomResult = ResultType.Nothing;
                        break;
                    case 1:
                        randomResult = ResultType.Teleport;
                        break;
                    case 2:
                        randomResult = ResultType.LoseLife;
                        break;
                    case 3:
                        randomResult = ResultType.GetTargetDirection;
                        break;
                    case 4:
                        randomResult = ResultType.GetLife;
                        break;
                    default:
                        randomResult = ResultType.GameWon;
                        break;
                }
                random = rnd.Next(0, 100);

                // Be sure to test it with empty and long text too.
                if (i == 0)
                {
                    alternatives.Add(new Alternative()
                    {
                        Text = "",
                        TrialResult = new TrialResult()
                        {
                            ResultType = randomResult,
                            Text = ""
                        }
                    });
                }
                else if (i == 1)
                {
                    alternatives.Add(new Alternative()
                    {
                        Text = "it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. ",
                        TrialResult = new TrialResult()
                        {
                            ResultType = randomResult,
                            Text = "it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. it's a really long text. "
                        }
                    });
                }
                else
                {
                    alternatives.Add(new Alternative()
                    {
                        Text = i + ". test alternative's text" + random,
                        TrialResult = new TrialResult()
                        {
                            ResultType = randomResult,
                            Text = "alternative result's test text" + random
                        }
                    });
                }

            }
            return alternatives;
        }
        #endregion

        #region Check data

        private void CheckGameResultViewModel(String gameTitle, String userName, GameResultViewModel model)
        {
            Game game = _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                                     .Include(g => g.GameWonResult)
                                     .Include(g => g.GameLostResult)
                                     .Include(g => g.Prelude)
                                     .FirstOrDefault();
            Assert.Equal(gameTitle, model.GameTitle);
            Assert.Equal(game.Prelude.Text, model.Prelude);
            Assert.Equal(game.GameWonResult.Text, model.GameWonResult);
            Assert.Equal(game.GameLostResult.Text, model.GameLostResult);
            Assert.Equal(game.Summary, model.Summary);
        }

        private void CheckGameResults(String userName, String gameTitle, String gameWonResult, String gameLostResult, 
            String prelude, String summary)
        {
            Game game = _context.Game.Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                                     .Include(g => g.GameLostResult)
                                     .Include(g => g.GameLostResult)
                                     .Include(g => g.Prelude)
                                     .FirstOrDefault();
            Assert.Equal(gameWonResult, game.GameWonResult.Text);
            Assert.Equal(gameLostResult, game.GameLostResult.Text);
            Assert.Equal(prelude, game.Prelude.Text);
            Assert.Equal(summary, game.Summary);
        }

        private void CheckTrial(IList<Alternative> alternatives, String trialText, Field field)
        {
            Assert.Equal(trialText, field.Trial.Text);
            for(int i = 0; i < 4; ++i)
            {
                Assert.Equal(alternatives[i].Text, field.Trial.Alternatives[i].Text);
                Assert.Equal(alternatives[i].TrialResult.Text, field.Trial.Alternatives[i].TrialResult.Text);
                Assert.Equal(alternatives[i].TrialResult.ResultType, field.Trial.Alternatives[i].TrialResult.ResultType);
            }
        }

        private void TestFieldOnEveryTypeOfWayDirectionsCode(Game game, int rowNumber, int colNumber, String gameTitle, String userName)
        {
            // Test with 0110 way directions code.
            CheckWayDirectionsCodeOnField(game, 0110, userName, gameTitle, rowNumber, colNumber);

            // Test with 0111 way directions code.
            CheckWayDirectionsCodeOnField(game, 0111, userName, gameTitle, rowNumber, colNumber);

            // Test with 0011 way directions code.
            CheckWayDirectionsCodeOnField(game, 0011, userName, gameTitle, rowNumber, colNumber);


            // Test with 1110 way directions code.
            CheckWayDirectionsCodeOnField(game, 1110, userName, gameTitle, rowNumber, colNumber);

            // Test with 1111 way directions code.
            CheckWayDirectionsCodeOnField(game, 1111, userName, gameTitle, rowNumber, colNumber);

            // Test with 1011 way directions code.
            CheckWayDirectionsCodeOnField(game, 1011, userName, gameTitle, rowNumber, colNumber);

            // Test with 1100 way directions code.
            CheckWayDirectionsCodeOnField(game, 1100, userName, gameTitle, rowNumber, colNumber);

            // Test with 1101 way directions code.
            CheckWayDirectionsCodeOnField(game, 1101, userName, gameTitle, rowNumber, colNumber);

            // Test with 1001 way directions code.
            CheckWayDirectionsCodeOnField(game, 1001, userName, gameTitle, rowNumber, colNumber);

            // Test with 0100 way directions code.
            CheckWayDirectionsCodeOnField(game, 0100, userName, gameTitle, rowNumber, colNumber);

            // Test with 0101 way directions code.
            CheckWayDirectionsCodeOnField(game, 0101, userName, gameTitle, rowNumber, colNumber);

            // Test with 0001 way directions code.
            CheckWayDirectionsCodeOnField(game, 0001, userName, gameTitle, rowNumber, colNumber);


            // Test with 0010 way directions code.
            CheckWayDirectionsCodeOnField(game, 0010, userName, gameTitle, rowNumber, colNumber);

            // Test with 1000 way directions code.
            CheckWayDirectionsCodeOnField(game, 1000, userName, gameTitle, rowNumber, colNumber);

            // Test with 1010 way directions code.
            CheckWayDirectionsCodeOnField(game, 1010, userName, gameTitle, rowNumber, colNumber);

            // Test with 0000 way directions code.
            CheckWayDirectionsCodeOnField(game, 0000, userName, gameTitle, rowNumber, colNumber);

        }

        private void CheckWayDirectionsCodeOnField(Game game, int wayDirectionsCode, String userName,
            String gameTitle, int rowNumber, int colNumber)
        {
            game.CurrentWayDirectionsCode = wayDirectionsCode;
            _context.SaveChanges();
            gameEditorService.SetExitRoads(userName, gameTitle, rowNumber, colNumber);
            Field field = _context.Field.Where(f => f.UserName == userName && f.GameTitle == gameTitle
                        && f.RowNumber == rowNumber && f.ColNumber == colNumber)
                    .FirstOrDefault();

            if (wayDirectionsCode % 10 == 0) Assert.False(field.IsLeftWay);
            else Assert.True(field.IsLeftWay);

            if ((wayDirectionsCode / 10) % 10 == 0) Assert.False(field.IsDownWay);
            else Assert.True(field.IsDownWay);

            if ((wayDirectionsCode / 100) % 10 == 0) Assert.False(field.IsRightWay);
            else Assert.True(field.IsRightWay);

            if ((wayDirectionsCode / 1000) % 10 == 0) Assert.False(field.IsUpWay);
            else Assert.True(field.IsUpWay);
        }


        // Check only the data that's needed to show the map. (In this case, we don't need more infromation about the fields.)
        private void CheckMapViewModelMap(ICollection<MapRow> map, String userName, String gameTitle)
        {
            ICollection<MapRow> mapFormDatabase = _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                .Include(g => g.Map)
                .ThenInclude(m => m.Row)
                .Select(g => g.Map)
                .FirstOrDefault();

            foreach (MapRow mapRow in map)
            {
                foreach (Field field in mapRow.Row)
                {
                    Field fieldFromDatabase = GetFieldFromMap(mapFormDatabase, field.RowNumber, field.ColNumber);
                    Assert.Equal(fieldFromDatabase.ID, field.ID);
                    Assert.Equal(fieldFromDatabase.UserName, field.UserName);
                    Assert.Equal(fieldFromDatabase.GameTitle, field.GameTitle);
                    Assert.Equal(fieldFromDatabase.RowNumber, field.RowNumber);
                    Assert.Equal(fieldFromDatabase.ColNumber, field.ColNumber);
                    Assert.Equal(fieldFromDatabase.IsRightWay, field.IsRightWay);
                    Assert.Equal(fieldFromDatabase.IsLeftWay, field.IsLeftWay);
                    Assert.Equal(fieldFromDatabase.IsUpWay, field.IsUpWay);
                    Assert.Equal(fieldFromDatabase.IsDownWay, field.IsDownWay);
                }
            }
        }

        private void CheckEditedMap(ICollection<MapRow> map, String userName, String gameTitle)
        {
            ICollection<MapRow> mapFormDatabase = _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                .Include(g => g.Map)
                .ThenInclude(m => m.Row)
                .ThenInclude(f => f.Trial)
                .ThenInclude(f => f.Alternatives)
                .ThenInclude(a => a.TrialResult)
                .Select(g => g.Map)
                .FirstOrDefault();

            foreach (MapRow mapRow in map)
            {
                foreach (Field field in mapRow.Row)
                {
                    Field fieldFromDatabase = GetFieldFromMap(mapFormDatabase, field.RowNumber, field.ColNumber);
                    Assert.Equal(fieldFromDatabase.ID, field.ID);
                    Assert.Equal(fieldFromDatabase.Text, field.Text);
                    Assert.Equal(fieldFromDatabase.UserName, field.UserName);
                    Assert.Equal(fieldFromDatabase.GameTitle, field.GameTitle);
                    Assert.Equal(fieldFromDatabase.RowNumber, field.RowNumber);
                    Assert.Equal(fieldFromDatabase.ColNumber, field.ColNumber);
                    Assert.Equal(fieldFromDatabase.IsRightWay, field.IsRightWay);
                    Assert.Equal(fieldFromDatabase.IsLeftWay, field.IsLeftWay);
                    Assert.Equal(fieldFromDatabase.IsUpWay, field.IsUpWay);
                    Assert.Equal(fieldFromDatabase.IsDownWay, field.IsDownWay);
                    if (fieldFromDatabase.Trial != null)
                    {
                        Assert.Equal(fieldFromDatabase.Trial.Text, field.Trial.Text);
                        Assert.Equal(fieldFromDatabase.Trial.ID, field.Trial.ID);
                        for (int i = 0; i < 4; ++i)
                        {
                            Assert.Equal(fieldFromDatabase.Trial.Alternatives[i].Text, field.Trial.Alternatives[i].Text);
                            Assert.Equal(fieldFromDatabase.Trial.Alternatives[i].TrialResult.ResultType,
                                field.Trial.Alternatives[i].TrialResult.ResultType);
                            Assert.Equal(fieldFromDatabase.Trial.Alternatives[i].TrialResult.Text,
                                field.Trial.Alternatives[i].TrialResult.Text);
                            Assert.Equal(fieldFromDatabase.Trial.Alternatives[i].ID, field.Trial.Alternatives[i].ID);
                            Assert.Equal(fieldFromDatabase.Trial.Alternatives[i].TrialResult.ID,
                                field.Trial.Alternatives[i].TrialResult.ID);
                        }
                    }
                    else
                    {
                        Assert.Null(field.Trial);
                    }

                }
            }
        }

        private void CheckField(Field field, String userName, String gameTitle)
        {
            Field fieldFromDatabase = GetField(gameTitle, userName, field.RowNumber, field.ColNumber);
            Assert.Equal(fieldFromDatabase.ID, field.ID);
            Assert.Equal(fieldFromDatabase.Text, field.Text);
            Assert.Equal(fieldFromDatabase.UserName, field.UserName);
            Assert.Equal(fieldFromDatabase.GameTitle, field.GameTitle);
            Assert.Equal(fieldFromDatabase.RowNumber, field.RowNumber);
            Assert.Equal(fieldFromDatabase.ColNumber, field.ColNumber);
            Assert.Equal(fieldFromDatabase.IsRightWay, field.IsRightWay);
            Assert.Equal(fieldFromDatabase.IsLeftWay, field.IsLeftWay);
            Assert.Equal(fieldFromDatabase.IsUpWay, field.IsUpWay);
            Assert.Equal(fieldFromDatabase.IsDownWay, field.IsDownWay);
            if (fieldFromDatabase.Trial != null)
            {
                Assert.Equal(fieldFromDatabase.Trial.Text, field.Trial.Text);
                Assert.Equal(fieldFromDatabase.Trial.ID, field.Trial.ID);
                for (int i = 0; i < 4; ++i)
                {
                    Assert.Equal(fieldFromDatabase.Trial.Alternatives[i].Text, field.Trial.Alternatives[i].Text);
                    Assert.Equal(fieldFromDatabase.Trial.Alternatives[i].TrialResult.ResultType,
                        field.Trial.Alternatives[i].TrialResult.ResultType);
                    Assert.Equal(fieldFromDatabase.Trial.Alternatives[i].TrialResult.Text,
                        field.Trial.Alternatives[i].TrialResult.Text);
                    Assert.Equal(fieldFromDatabase.Trial.Alternatives[i].ID, field.Trial.Alternatives[i].ID);
                    Assert.Equal(fieldFromDatabase.Trial.Alternatives[i].TrialResult.ID,
                        field.Trial.Alternatives[i].TrialResult.ID);
                }
            }
            else
            {
                Assert.Null(field.Trial);
            }

        }

        private void CheckDefaultMap(ICollection<MapRow> map, String userName, String gameTitle)
        {
            //Sort map
            foreach (MapRow mapRow in map)
            {
                mapRow.Row = mapRow.Row.OrderBy(r => r.ColNumber).ToList();
            }
            int i = 0;
            foreach (MapRow mapRow in map)
            {
                int j = 0;
                foreach (Field field in mapRow.Row)
                {
                    Assert.Equal(j, field.ColNumber);
                    Assert.Equal(i, field.RowNumber);
                    j++;

                    Assert.Null(field.Trial);

                    Assert.Equal(userName, field.UserName);
                    Assert.Equal(gameTitle, field.GameTitle);
                    Assert.Null(field.Image);
                    Assert.False(field.IsDownWay);
                    Assert.False(field.IsUpWay);
                    Assert.False(field.IsLeftWay);
                    Assert.False(field.IsRightWay);
                    Assert.Empty(field.Text);
                }
                i++;
            }
        }
        #endregion

        #region Getters

        private User GetUser(String userName)
        {
            return _context.User.Where(u => u.UserName == userName).FirstOrDefault();
        }
        private Tuple<List<String>, List<TrialResult>> GetTextsAndTrialResultsFromAlternatives(
            List<Alternative> alternatives)
        {
            List<String> alternativeTexts = new List<String>();
            List<TrialResult> trialResults = new List<TrialResult>();
            foreach (Alternative alternative in alternatives)
            {
                alternativeTexts.Add(alternative.Text);
                trialResults.Add(alternative.TrialResult);
            }
            return new Tuple<List<string>, List<TrialResult>>(alternativeTexts, trialResults);
        }

        private int GetWayDirectionsCode(String gameTitle, String userName)
        {
            return _context.Game
                .Where(g => g.Title == "GameTitle" && g.Owner.UserName == "TestUser11")
                .FirstOrDefault()
                .CurrentWayDirectionsCode;
        }

        private Field GetFieldFromMap(ICollection<MapRow> map, int rowNumber, int colNumber)
        {
            foreach (MapRow mapRow in map)
            {
                foreach (Field field in mapRow.Row)
                {
                    if (field.ColNumber == colNumber && field.RowNumber == rowNumber)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        private Field GetField(String gameTitle, String ownerName, int rowNumber, int colNumber)
        {
            return _context.Field.Where(f => f.GameTitle == gameTitle && f.UserName == ownerName && f.ColNumber == colNumber
                    && f.RowNumber == rowNumber)
                    .Include(f => f.Trial)
                    .ThenInclude(t => t.Alternatives)
                    .ThenInclude(a => a.TrialResult)
                    .FirstOrDefault();
        }

        private String GetFieldText(String userName, String gameTitle, int colNumber, int rowNulber)
        {
            return _context.Field.Where(f => f.UserName == userName && f.GameTitle == gameTitle
                && f.ColNumber == colNumber && f.RowNumber == rowNulber)
                .FirstOrDefault().Text;
        }

        private Game GetTestGame(String gameTitle, String userName)
        {
            return _context.Game
                .Include(g => g.Owner)
                .Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                .Include(g => g.Map)
                .ThenInclude(m => m.Row)
                .FirstOrDefault();
        }

        private ICollection<MapRow> GetMap(String userName, String gameTitle)
        {
            return _context.Game
                        .Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                        .Include(g => g.Map)
                        .ThenInclude(m => m.Row)
                        .ThenInclude(f => f.Trial)
                        .ThenInclude(t => t.Alternatives)
                        .ThenInclude(a => a.TrialResult)
                        .Select(g => g.Map)
                        .FirstOrDefault();
        }

        private ICollection<MapRow> GetMapWhitoutTrials(String userName, String gameTitle)
        {
            return _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                                .Include(g => g.Map)
                                .ThenInclude(m => m.Row)
                                .Select(g => g.Map)
                                .FirstOrDefault();
        }

        private String GetLongTestString()
        {
            Random rnd = new Random();
            String result = "This is a long string for testing with some unique random numbers. ";
            for (int i = 0; i < 100; ++i)
            {
                result += "This is some string content with a unique random number: " + rnd.Next(1, 1000) + ". ";
            }
            return result;
        }

        private String GetAverageTestString()
        {
            Random rnd = new Random();
            return "This is an average long string with random number " + rnd.Next(1, 1000) + ".";
        }

        private List<Alternative> GetAlternativesFromTextsAndTrialResults(List<String> alternativeTexts, 
            List<TrialResult> trialResults)
        {
            List<Alternative> alternatives = new List<Alternative>();
            for(int i = 0; i < 4; ++i)
            {
                alternatives.Add(new Alternative()
                {
                    Text = alternativeTexts[i],
                    TrialResult = trialResults[i]
                });
            }
            return alternatives;
        }

        #endregion

        #region Setters

        private void SetFieldDirections(String userName, String gameTitle, int rowNumber, int colNumber, 
            Boolean isUpWay, Boolean isRightWay, Boolean isDownWay, Boolean isLeftWay)
        {
            Field field = _context.Field.Where(f => f.UserName == userName && f.GameTitle == gameTitle 
                                                && f.ColNumber == colNumber && f.RowNumber == rowNumber)
                                        .FirstOrDefault();
            field.IsUpWay = isUpWay;
            field.IsRightWay = isRightWay;
            field.IsDownWay = isDownWay;
            field.IsLeftWay = isLeftWay;
            _context.SaveChanges();
        }

        private void SetStartField(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                         .FirstOrDefault()
                         .StartField =
                         _context.Field.Where(f => f.UserName == userName && f.GameTitle == gameTitle &&
                         f.ColNumber == colNumber && f.RowNumber == rowNumber)
                         .FirstOrDefault();
            _context.SaveChanges();
        }

        private void SetTargetField(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                         .FirstOrDefault()
                         .TargetField =
                         _context.Field.Where(f => f.UserName == userName && f.GameTitle == gameTitle &&
                         f.ColNumber == colNumber && f.RowNumber == rowNumber)
                         .FirstOrDefault();
            _context.SaveChanges();
        }

        #endregion

        #endregion

    }
}
