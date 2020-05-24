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
using SQLitePCL;
using AdventureGameEditor.Models.DatabaseModels.Gameplay;

namespace AdventureGameEditor.UnitTests
{
    public class TestUtilities : IDisposable
    {
        public readonly AdventureGameEditorContext context;

        public TestUtilities()
        {
            // Initialize context.
            var contextOptions = new DbContextOptionsBuilder<AdventureGameEditorContext>()
                .UseInMemoryDatabase("AdventureGameEditorContext")
                .Options;
            this.context = new AdventureGameEditorContext(contextOptions);
            this.context.Database.EnsureCreated();
            this.context.SaveChanges();
        }

        public void Dispose()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }

        #region Generate data for testing

        public void InitializeTestUsers()
        {
            List<User> users = new List<User>();
            for (int i = 0; i < 50; ++i)
            {
                if (!context.User.Any(u => u.UserName == "TestUser" + i))
                {
                    users.Add(new User()
                    {
                        UserName = "GameplayTestUser" + i,
                        NickName = "TestUSerNick" + i,
                        Email = "testuser" + i + "@gmail.com"
                    });
                }
            }
            context.User.AddRange(users);
            context.SaveChanges();
        }

        public void InitializeGame(String userName, String gameTitle, int mapSize, Visibility visibility)
        {
            InitializeTestUsers();
            Game game = context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle).FirstOrDefault();
            if (game != null)
            {
                context.Game.Remove(game);
                context.SaveChanges();
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
            context.Game.Add(
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
            context.SaveChanges();
        }

        public void MakeGameWithCorrectMap(String userName, String gameTitle, Visibility visibility)
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

        public void MakeGameWithLargeCorrectMap(String userName, String gameTitle, Visibility visibility)
        {
            MakeTestGame(gameTitle, 15, visibility, userName);

            // Set Corners.
            SetFieldDirections(userName, gameTitle, 0, 0, false, true, true, false);
            SetFieldDirections(userName, gameTitle, 0, 14, false, false, true, true);
            SetFieldDirections(userName, gameTitle, 14, 0, true, true, false, false);
            SetFieldDirections(userName, gameTitle, 14, 14, true, false, false, true);

            // Set edges.
            for (int i = 1; i < 14; ++i)
            {
                SetFieldDirections(userName, gameTitle, 0, i, false, true, true, true);
                SetFieldDirections(userName, gameTitle, 14, i, true, true, false, true);
                SetFieldDirections(userName, gameTitle, i, 0, true, true, true, false);
                SetFieldDirections(userName, gameTitle, i, 14, true, false, true, true);
            }

            // Set fields inside the map.
            for (int i = 1; i < 14; ++i)
            {
                for (int j = 1; j < 14; ++j)
                {
                    SetFieldDirections(userName, gameTitle, i, j, true, true, true, true);
                }
            }
        }

        public GameResult GenerateGameResult(String userName, String gameTitle, Boolean isGameWonResult, String text)
        {
            return new GameResult()
            {
                Owner = context.User.Where(u => u.UserName == userName).FirstOrDefault(),
                GameTitle = gameTitle,
                IsGameWonResult = isGameWonResult,
                Text = text
            };
        }

        public Prelude GeneratePrelude(String userName, String gameTitle, String text)
        {
            return new Prelude()
            {
                Text = text,
                GameTitle = gameTitle,
                Owner = context.User.Where(u => u.UserName == userName).FirstOrDefault()
            };
        }

        public void MakeDefaultTestGames()
        {
            InitializeTestUsers();
            for (int i = 0; i < 50; ++i)
            {
                InitializeGame("TestUser" + i, "GameTitle" + i, (i % 12) + 3, Visibility.Everyone);
            }
        }

        public void MakeUniqueTestGames(int gameCount, Boolean isAddAllContent)
        {
            InitializeTestUsers();
            for (int i = 50; i < 50 + gameCount; ++i)
            {
                String userName = "TestUserGameplay" + (i - 50);
                String gameTitle = "GameTitleGameplay" + i;
                Random rnd = new Random();
                if (!context.Game.Any(g => g.Owner.UserName == userName && g.Title == gameTitle))
                {
                    int random = rnd.Next(1, 4);
                    Visibility randomVisibility = Visibility.Everyone;
                    switch (random)
                    {
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

                    ICollection<MapRow> map = context.Game
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
                        Owner = context.User.Where(u => u.UserName == userName).FirstOrDefault(),
                        GameTitle = gameTitle,
                        IsGameWonResult = false,
                        Text = "Test game lost result text."
                    };
                    game.GameWonResult = new GameResult()
                    {
                        Owner = context.User.Where(u => u.UserName == userName).FirstOrDefault(),
                        GameTitle = gameTitle,
                        IsGameWonResult = true,
                        Text = "Test game won result text."
                    };
                    game.Prelude = new Prelude()
                    {
                        Text = "Tet prelude text.",
                        GameTitle = gameTitle,
                        Owner = context.User.Where(u => u.UserName == userName).FirstOrDefault()
                    };
                    context.SaveChanges();
                }
            }
        }

        public void DeleteGame(String userName, String gameTitle)
        {
            Game game = context.Game.Where(g => g.Title == gameTitle && g.Owner.UserName == userName).FirstOrDefault();
            foreach(Field field in context.Field.Where(f => f.GameTitle == gameTitle && f.UserName == userName).ToList())
            {
                context.Field.Remove(field);
            }
            foreach(GameResult gameResult in context.GameResult.Where(r => r.GameTitle == gameTitle && 
                r.Owner.UserName == userName).ToList())
            {
                context.GameResult.Remove(gameResult);
            }
            foreach(GameplayData data in context.GameplayData.Where(d => d.GameTitle == gameTitle).ToList())
            {
                context.GameplayData.Remove(data);
            }
        }

        public void MakeTestGame(String gameTitle, int tableSize, Visibility visibility, String userName)
        {
            InitializeTestUsers();
            InitializeGame(userName, gameTitle, tableSize, visibility);

            ICollection<MapRow> map = context.Game
                                              .Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                                              .Select(g => g.Map)
                                              .FirstOrDefault();
            map = EditDefaultMap(map, false);
            context.SaveChanges();
        }

        // Edits the map's ways randomly. (These are empty by default.)
        public ICollection<MapRow> EditDefaultMap(ICollection<MapRow> map, Boolean isAddAllContents)
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

        public Trial GenerateTrial()
        {
            Random rnd = new Random();
            return new Trial()
            {
                Text = "Test trial text. Test trial text. Test trial text. Test trial text." + rnd.Next(1, 10000),
                Alternatives = GenerateAlternatives()
            };
        }

        public List<Alternative> GenerateAlternatives()
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

        #region Getters

        public User GetUser(String userName)
        {
            return context.User.Where(u => u.UserName == userName).FirstOrDefault();
        }
        public Tuple<List<String>, List<TrialResult>> GetTextsAndTrialResultsFromAlternatives(
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

        public int GetWayDirectionsCode(String gameTitle, String userName)
        {
            return context.Game
                .Where(g => g.Title == "GameTitle" && g.Owner.UserName == "TestUser11")
                .FirstOrDefault()
                .CurrentWayDirectionsCode;
        }

        public Field GetFieldFromMap(ICollection<MapRow> map, int rowNumber, int colNumber)
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

        public Field GetField(String gameTitle, String ownerName, int rowNumber, int colNumber)
        {
            return context.Field.Where(f => f.GameTitle == gameTitle && f.UserName == ownerName && f.ColNumber == colNumber
                    && f.RowNumber == rowNumber)
                    .Include(f => f.Trial)
                    .ThenInclude(t => t.Alternatives)
                    .ThenInclude(a => a.TrialResult)
                    .FirstOrDefault();
        }

        public String GetFieldText(String userName, String gameTitle, int colNumber, int rowNulber)
        {
            return context.Field.Where(f => f.UserName == userName && f.GameTitle == gameTitle
                && f.ColNumber == colNumber && f.RowNumber == rowNulber)
                .FirstOrDefault().Text;
        }

        public Game GetTestGame(String gameTitle, String userName)
        {
            return context.Game
                .Include(g => g.Owner)
                .Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                .Include(g => g.Map)
                .ThenInclude(m => m.Row)
                .FirstOrDefault();
        }

        public ICollection<MapRow> GetMap(String userName, String gameTitle)
        {
            return context.Game
                        .Where(g => g.Title == gameTitle && g.Owner.UserName == userName)
                        .Include(g => g.Map)
                        .ThenInclude(m => m.Row)
                        .ThenInclude(f => f.Trial)
                        .ThenInclude(t => t.Alternatives)
                        .ThenInclude(a => a.TrialResult)
                        .Select(g => g.Map)
                        .FirstOrDefault();
        }

        public ICollection<MapRow> GetMapWhitoutTrials(String userName, String gameTitle)
        {
            return context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                                .Include(g => g.Map)
                                .ThenInclude(m => m.Row)
                                .Select(g => g.Map)
                                .FirstOrDefault();
        }

        public String GetLongTestString()
        {
            Random rnd = new Random();
            String result = "This is a long string for testing with some unique random numbers. ";
            for (int i = 0; i < 100; ++i)
            {
                result += "This is some string content with a unique random number: " + rnd.Next(1, 1000) + ". ";
            }
            return result;
        }

        public String GetAverageTestString()
        {
            Random rnd = new Random();
            return "This is an average long string with random number " + rnd.Next(1, 1000) + ".";
        }

        public List<Alternative> GetAlternativesFromTextsAndTrialResults(List<String> alternativeTexts,
            List<TrialResult> trialResults)
        {
            List<Alternative> alternatives = new List<Alternative>();
            for (int i = 0; i < 4; ++i)
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

        public void SetFieldDirections(String userName, String gameTitle, int rowNumber, int colNumber,
            Boolean isUpWay, Boolean isRightWay, Boolean isDownWay, Boolean isLeftWay)
        {
            Field field = context.Field.Where(f => f.UserName == userName && f.GameTitle == gameTitle
                                                && f.ColNumber == colNumber && f.RowNumber == rowNumber)
                                        .FirstOrDefault();
            field.IsUpWay = isUpWay;
            field.IsRightWay = isRightWay;
            field.IsDownWay = isDownWay;
            field.IsLeftWay = isLeftWay;
            context.SaveChanges();
        }

        public void SetStartField(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                         .FirstOrDefault()
                         .StartField =
                         context.Field.Where(f => f.UserName == userName && f.GameTitle == gameTitle &&
                         f.ColNumber == colNumber && f.RowNumber == rowNumber)
                         .FirstOrDefault();
            context.SaveChanges();
        }

        public void SetTargetField(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                         .FirstOrDefault()
                         .TargetField =
                         context.Field.Where(f => f.UserName == userName && f.GameTitle == gameTitle &&
                         f.ColNumber == colNumber && f.RowNumber == rowNumber)
                         .FirstOrDefault();
            context.SaveChanges();
        }

        #endregion
    }

}
