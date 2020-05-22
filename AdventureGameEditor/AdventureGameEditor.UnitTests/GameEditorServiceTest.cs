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
using System.Threading;
using SQLitePCL;

namespace AdventureGameEditor.UnitTests
{
    
    public class GameEditorServiceTest : IDisposable
    {
        private readonly AdventureGameEditorContext _context;
        private readonly IGameEditorService gameEditorService;
        

        public  GameEditorServiceTest()
        {
            // Initialize context.
            var contextOptions = new DbContextOptionsBuilder<AdventureGameEditorContext>()
                .UseInMemoryDatabase("AdventureGameEditorContext")
                .Options;
            _context = new AdventureGameEditorContext(contextOptions);
            _context.Database.EnsureCreated();
            gameEditorService = new GameEditorService(_context);
            InitializeTestUsers();
            _context.SaveChanges();
        }
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Test game creation
        [Fact]
        public void TestInitialize()
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
            for(int i = 50; i < 100; ++i)
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
            for(int i = 1; i < 14; ++i)
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
            for(int i = 50; i < 100; ++i)
            {
                String gameTitle = "GameTitle" + i;
                String userName = "TestUser" + (i - 50);
                ICollection<MapRow> map = GetMap(userName, gameTitle);
                foreach(MapRow mapRow in map)
                {
                    foreach(Field field in mapRow.Row)
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
                        if(field.ColNumber % 3 == 0 && field.RowNumber % 3 == 0)
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

        #region Helper functions

        #region Generate data for testing

        private void InitializeTestUsers()
        {
            for(int i = 0; i < 50; ++i)
            {
                if(!_context.User.Any(u => u.UserName == "TestUser" + i))
                {
                    _context.User.Add(new User()
                    {
                        UserName = "TestUser" + i,
                        NickName = "TestUSerNick" + i,
                        Email = "testiser" + i + "@gmail.com"
                    });
                }
            }
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
                InitializeGame(userName, gameTitle, rnd.Next(3, 16), randomVisibility);
                ICollection<MapRow> map = _context.Game
                                              .Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                                              .Select(g => g.Map)
                                              .FirstOrDefault();
                map = EditDefaultMap(map, isAddAllContent);
                Game game = GetTestGame(gameTitle, userName);
                game.StartField = GetField(gameTitle, userName, rnd.Next(0, 15), rnd.Next(0, 15));
                game.TargetField = GetField(gameTitle, userName, rnd.Next(0, 15), rnd.Next(0, 15));
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
