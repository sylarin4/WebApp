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
    public class GameEditorService : IGameEditorService
    {
        #region Attributes

        protected readonly AdventureGameEditorContext _context;

        #endregion

        #region Constructor
        public GameEditorService(AdventureGameEditorContext context)
        {
            _context = context;
        }

        #endregion

        #region Create game
        public Boolean InicializeGame(String title, int mapSize, Visibility visibility, User owner)
        {
            if (_context.Game.Any(game => game.Title == title)) return false;

            // Initialize a map.            
            List <MapRow> map = new List<MapRow>();
            for(int i = 0; i < mapSize; ++i)
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
                            Owner = _context.User.Where(user => user.Id == owner.Id).FirstOrDefault(),
                            GameTitle = title,
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

            // Test writeing on console.
            foreach(MapRow item in map)
            {
                foreach(Field field in item.Row)
                {
                    Trace.Write(field.Text + " ");
                }
                Trace.Write("\n");
            }

            // Save the initialized game to the database.
            _context.Game.Add(
                new Game
                {
                    Title = title,
                    Visibility = visibility,
                    TableSize = mapSize,
                    PlayCounter = 0,
                    Owner = owner,
                    Map = map,
                    CurrentWayDirectionsCode = 0
                }
                );
            _context.SaveChanges();
            return true;
        }

        #endregion

        #region Create map

        #region // ---------- Getters ---------- //
        public MapViewModel GetMapViewModel(String userName, String gameTitle)
        {
            Game game = GetGameAtTitle(userName, gameTitle);
            return new MapViewModel
            {
                MapSize = game.TableSize,
                GameTitle = game.Title,
                Map = SortMap(game.Map.ToList())
            };
        }

        public MapPieceViewModel GetFieldViewModel(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            return new MapPieceViewModel()
            {
                GameTitle = gameTitle,
                Field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber)
            };
        }

        public FileResult ImageForMap(int? wayDirectionsCode)
        {
            // TODO: do it for not only the test theme
            if (wayDirectionsCode == null) wayDirectionsCode = 0;
            Byte[] imageContent = GetImage((int)wayDirectionsCode, MapTheme.Test);
            return new FileContentResult(imageContent, "image/png");
        }

        public String GetTextAtCoordinate(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            return field.Text;
        }
        #endregion

        #region // ---------- Setters --------- //
        public void SetExitRoads(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            int wayDirectionsCode = GetCurrentWayDirectionsCode(userName, gameTitle);
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            //Trace.WriteLine("Is field null?: " + field == null);
            if(field != null)
            {
                // Set direction "left".
                if(wayDirectionsCode % 10 == 0)
                    field.IsLeftWay = false;
                else
                    field.IsLeftWay = true;

                // Set direction "down".
                if((wayDirectionsCode/ 10)%10 == 0)
                    field.IsDownWay = false;
                else
                    field.IsDownWay = true;

                // Set direction "right".
                if((wayDirectionsCode/100)%10 == 0)
                    field.IsRightWay = false;
                else
                    field.IsRightWay = true;

                // Set direction "up".
                if((wayDirectionsCode/1000)%10 == 0)
                    field.IsUpWay = false;
                else
                    field.IsUpWay = true;
                Trace.WriteLine("\n\n\nWayDirectionsCode: " + wayDirectionsCode);
                Trace.WriteLine("IsUpWay: " + field.IsUpWay + " IsDownWay: " + field.IsDownWay +
                    " IsLeftWay: " + field.IsLeftWay + " IsRightWay: " + field.IsRightWay);
                _context.SaveChanges();
            }
        }

        public void SetCurrentWayDirectionsCode(String userName, String gameTitle, int newWayDirectionsCode)
        {
            Game game = GetGameAtTitle(userName, gameTitle);
            game.CurrentWayDirectionsCode = newWayDirectionsCode;
            Trace.WriteLine("The new currentwayDirectionsCode: " + GetCurrentWayDirectionsCode(userName, gameTitle));
            _context.SaveChanges();
        }

        #endregion

        #endregion

        #region Create map content

        #region //---------- Setters ----------//

        #region Initializing
        //Currently not used.
        public Trial InitializeTrial(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            // Initialize trial.
            List<Alternative> alternatives = new List<Alternative>();
            
            for(int i = 0; i < 4; ++i)
            {
                alternatives.Add(new Alternative()
                {
                    Text = "default",
                    TrialResult = new TrialResult()
                    {
                        Text = "",
                        ResultType = ResultType.Nothing
                    }
                });
            }
                        
            Trial trial = new Trial()
            {
                TrialType = TrialType.MultipleChoice,
                Alternatives = alternatives
            };
            // Save initialization.
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            field.Trial = trial;
            for(int i = 0; i < 4; ++i)
            {
                Trace.WriteLine(GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber).Trial.Alternatives.ElementAt(i).Text);
            }
            _context.SaveChanges();
            return trial;
        }

        //Currently not used.
        public List<Alternative> InitializeAlternatives(int count)
        {
            List<Alternative> alternatives = new List<Alternative>();
            for(int i = 0; i < count; ++i)
            {
                alternatives.Add(new Alternative()
                {
                    Text = "test text",
                    TrialResult = new TrialResult()
                    {
                        ResultType = ResultType.Nothing,
                        Text = "test text"
                    }
                });
            }
            return alternatives;
        }

        public List<String> InitializeAlternativeTexts(int count)
        {
            List<String> alternativeTexts = new List<String>();
            for(int i = 0; i < count; ++i)
            {
                alternativeTexts.Add("test text");
            }
            return alternativeTexts;
        }

        public List<TrialResult> InitializeTrialResults(int count)
        {
            List<TrialResult> trialResults = new List<TrialResult>();
            for(int i = 0; i < count; ++i)
            {
                trialResults.Add(new TrialResult()
                {
                    ResultType = ResultType.Nothing,
                    Text = "test text"
                });
            }
            return trialResults;
        }
        #endregion

        #region Save 

         public void AddTextToAFieldAt(String userName, String gameTitle, int rowNumber, int colNumber, String text)
        {
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            field.Text = text;
            _context.SaveChanges();
        }

        public void SaveTrial(String userName, String gameTitle, int rowNumber, int colNumber, List<String> alternativeTexts, 
            List<TrialResult> alternativeTrialResults, TrialType trialType )
        {
            Trial trial = new Trial();
            trial.Alternatives = new List<Alternative>();
            trial.TrialType = trialType;
            // Test writing.
            foreach(String text in alternativeTexts)
            {
                Trace.WriteLine(text);
            }
            foreach(TrialResult result in alternativeTrialResults)
            {
                Trace.WriteLine(result.Text);
                Trace.WriteLine(result.ResultType);
            }

            for(int i = 0; i < alternativeTexts.Count; ++i)
            {
                trial.Alternatives.Add(new Alternative()
                {
                    Text = alternativeTexts[i],
                    TrialResult = alternativeTrialResults[i]
                });
            }
            // Somehow, the trial results are saved incorrectly. Check if they are filled to trial corretly, and how 
            // they were saved.
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            field.Trial = trial;
            _context.SaveChanges();
        }

        #endregion

        // Currently not used.
        public void AddNewAlternativeToForm(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            for(int i = 0; i < 10; ++i)
            {
                field.Trial.Alternatives.Add(new Alternative()
                {
                    Text = "",
                    TrialResult = new TrialResult()
                    {
                        ResultType = ResultType.Nothing,
                        Text = ""
                    }
                });
            }
            
        }
        #endregion


        #region //---------- Getters ----------// 
        public Trial GetTrial(String userName, String gameTitle, int colNumber, int rowNumber)
        {
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            // TODO: we have to include alternatives to the trial. we may store gametitle and owner's name for it. 
            // Look at in the hoework, maybe there's a special way for automatice it
            return field.Trial;
        }
        public List<MapRow> GetMap(String userName, String gameTitle)
        {
            return SortMap(_context.Game
                .Where(game => game.Owner.UserName == userName & game.Title == gameTitle)
                .Include(game => game.Map)
                .ThenInclude(map => map.Row)
                .Select(game => game.Map)
                .FirstOrDefault()
                .ToList());
        }

        public MapContentViewModel GetMapContentViewModel(String userName, String gameTitle)
        {
            Game game = GetGameAtTitle(userName, gameTitle);
            return new MapContentViewModel()
            {
                GameTitle = game.Title,
                Map = SortMap(game.Map.ToList()),
                MapSize = game.TableSize
            };
        }

        //Currently not used.
        // Get the data from the database we need and convert it to a FieldContentViewModel, then return it.
        public FieldContentViewModel GetFieldContentViewModel(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            // Load the field.
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);

            // Initializeing.
            List<String> alternativeTexts = new List<String>();
            List<TrialResult> alternativeTrialResults = new List<TrialResult>();
            TrialType trialType = TrialType.LuckTrial;

            // If trial wasn't created previously, initialize it.
            if(field.Trial == null)
            {
                alternativeTexts = InitializeAlternativeTexts(4);
                alternativeTrialResults = InitializeTrialResults(4);
            }

            // If trial was created previously, load that data.
            else
            {
                trialType = field.Trial.TrialType;
                foreach(Alternative alternative in field.Trial.Alternatives)
                {
                    alternativeTexts.Add(alternative.Text);
                    alternativeTrialResults.Add(alternative.TrialResult);
                }                
            }

            // Convert data to FieldContentViewModel type.
            FieldContentViewModel model = new FieldContentViewModel()
                {
                    GameTitle = gameTitle,
                    ColNumber = colNumber,
                    RowNumber = rowNumber,
                    TextContent = field.Text,
                    TrialType = trialType,
                    AlternativeTexts = alternativeTexts,
                    TrialResults = alternativeTrialResults
                };

            return model;
        }

        public FieldTrialContentViewModel GetFieldTrialContentViewModel(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            // Load the field.
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);

            // Initializeing.
            List<String> alternativeTexts = new List<String>();
            List<TrialResult> alternativeTrialResults = new List<TrialResult>();
            TrialType trialType = TrialType.LuckTrial;

            // If trial wasn't created previously, initialize it.
            if (field.Trial == null)
            {
                alternativeTexts = InitializeAlternativeTexts(4);
                alternativeTrialResults = InitializeTrialResults(4);
            }

            // If trial was created previously, load that data.
            else
            {
                trialType = field.Trial.TrialType;
                foreach (Alternative alternative in field.Trial.Alternatives)
                {
                    alternativeTexts.Add(alternative.Text);
                    alternativeTrialResults.Add(alternative.TrialResult);
                }
            }

            // Convert data to FieldContentViewModel type.
            FieldTrialContentViewModel model = new FieldTrialContentViewModel()
            {
                GameTitle = gameTitle,
                ColNumber = colNumber,
                RowNumber = rowNumber,
                TrialType = trialType,
                AlternativeTexts = alternativeTexts,
                TrialResults = alternativeTrialResults
            };

            return model;
        }


        #endregion

        #endregion

        #region Set start and target fields

        #region //---------- Setters ----------//

        // Save the start field which's coordinates are given in the function's attributes (rowNumber and colNumber).
        public void SaveStartField(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            Game game = GetGameAtTitle(userName, gameTitle);
            game.StartField = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            _context.SaveChanges();
        }

        public void SaveTargetField(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            Game game = GetGameAtTitle(userName, gameTitle);
            game.TargetField = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            _context.SaveChanges();
        }

        #endregion

        #endregion

        #region Show game details

        #region //---------- Getters ----------//

        public GameDetailsViewModel GetGameDetailsViewModel(String userName, String gameTitle)
        {
            Game game = GetGameAtTitle(userName, gameTitle);
            return new GameDetailsViewModel()
            {
                OwnerName = userName,
                Title= gameTitle,
                Visibility = game.Visibility,
                TableSize = game.TableSize,
                Map = SortMap(game.Map.ToList()),
                StartField = game.StartField,
                TargetField = game.TargetField,
                GameLostResult = game.GameLostResult.Text,
                GameWonResult = game.GameWonResult.Text,
                Prelude = game.Prelude.Text
            };
        }

        public FieldDetailsViewModel GetFieldDetailsViewModel(String userName, String gameTitle, int colNumber, int rowNumber)
        {
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            FieldDetailsViewModel fieldDetails = new FieldDetailsViewModel()
            {
                ColNumber = colNumber,
                RowNumber = rowNumber,
                TextContent = field.Text,
                IsTrial = field.Trial == null ? false : true
            };
            if(field.Trial != null)
            {
                fieldDetails.AlternativeTexts = new List<string>();
                fieldDetails.TrialResults = new List<TrialResult>();
                for(int i = 0; i < field.Trial.Alternatives.Count; ++i)
                {
                    fieldDetails.AlternativeTexts.Add(field.Trial.Alternatives.ElementAt(i).Text);
                    fieldDetails.TrialResults.Add(field.Trial.Alternatives.ElementAt(i).TrialResult);
                }
            }
            return fieldDetails;
        }

        #endregion

        #endregion

        #region Create game result

        public Boolean SaveGameResults(String userName, String gameTitle, 
            String gameWonResult, String gameLostResult, String prelude)
        {
            // If the results empty or not set, don't save them.
            if(gameLostResult == null || gameWonResult == null || gameLostResult == "" || gameWonResult == "") return false;
            
            Game game = GetGameAtTitle(userName, gameTitle);
            if (game == null) return false;

            game.GameLostResult = new GameResult()
            {
                Owner = game.Owner,
                GameTitle = gameTitle, 
                IsGameWonResult = false,
                Text = gameLostResult
            };
            game.GameWonResult = new GameResult()
            {
                Owner = game.Owner,
                GameTitle = gameTitle,
                IsGameWonResult = true,
                Text = gameWonResult
            };
            game.Prelude = new Prelude()
            {
                Text = prelude,
                Owner = game.Owner,
                GameTitle = gameTitle
            };
            _context.SaveChanges();
            return true;
        }

        #endregion

        #region Usually used getter functions

        public String GetFieldTextContent(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            String textContent =_context.Field
                .Where(field => field.Owner.UserName == userName && field.GameTitle == gameTitle
                && field.RowNumber == rowNumber && field.ColNumber == colNumber)
                .Select(field => field.Text)
                .FirstOrDefault();
            if(textContent == null)
            {
                return "";
            }
            else
            {
                return textContent;
            }
        }
        public Field GetField(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            return GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
        }

        #endregion

        #region Private helper functions

        private List<MapRow> SortMap(List<MapRow> map)
        {
            foreach(MapRow row in map)
            {
                row.Row = row.Row.OrderBy(row => row.ColNumber).ToList();
            }
            return map;
        }

        private Game GetGameAtTitle(String userName, String gameTitle)
        {
            return _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                .Include(g => g.Map)
                .ThenInclude(map => map.Row)
                .FirstOrDefault();
        }

        private Field GetFieldAtCoordinate(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            return _context.Field
                .Where(field => field.Owner.UserName == userName && field.GameTitle == gameTitle)
                .Where(field => field.ColNumber == colNumber && field.RowNumber == rowNumber)
                .Include(field => field.Trial)
                .ThenInclude(trial => trial.Alternatives)
                .ThenInclude(alternatives => alternatives.TrialResult)
                .FirstOrDefault();
        }

        private Byte[] GetImage(int wayDirections, MapTheme theme)
        {
            return _context.MapImage
                    .Where(image => image.WayDirectionsCode == wayDirections && image.Theme == theme)
                    .Select(image => image.Image)
                    .FirstOrDefault();
        }

        private int GetCurrentWayDirectionsCode(String userName, String gameTitle)
        {
            return _context.Game
                .Where(game => game.Owner.UserName == userName & game.Title == gameTitle)
                .Select(game => game.CurrentWayDirectionsCode)
                .FirstOrDefault();
        }

        #endregion
    }

}
