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
            if (_context.Game.Any(game => game.Title == title && game.Owner == owner)) return false;

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

        // ---------- Getters ---------- //
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

        // ---------- Setters --------- //
        public void AddTextToAFieldAt(String userName, String gameTitle, int rowNumber, int colNumber, String text)
        {
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            field.Text = text;
            _context.SaveChanges();
        }

        public void SetExitRoads(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            int wayDirectionsCode = GetCurrentWayDirectionsCode(userName, gameTitle);
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            //Trace.WriteLine("Is field null?: " + field == null);
            if(field != null)
            {
                // Set direction "up".
                if(wayDirectionsCode % 10 == 0)
                    field.IsUpWay = false;
                else
                    field.IsUpWay = true;

                // Set direction "right".
                if((wayDirectionsCode/ 10)%10 == 0)
                    field.IsRightWay = false;
                else
                    field.IsRightWay = true;

                // Set direction "down".
                if((wayDirectionsCode/100)%10 == 0)
                    field.IsDownWay = false;
                else
                    field.IsDownWay = true;

                // Set direction "left".
                if((wayDirectionsCode/1000)%10 == 0)
                    field.IsLeftWay = false;
                else
                    field.IsLeftWay = true;

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

        #region Create map content

        // ---------- Getters ---------- //

        public Trial GetTrial(String userName, String gameTitle, int colNumber, int rowNumber)
        {
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            // TODO: we have to include alternatives to the trial. we may store gametitle and owner's name for it. 
            // Look at in the hoework, maybe there's a special way for automatice it
            return field.Trial;
        }

        // ---------- Setters ---------- //

        public Trial InitializeTrial(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            // Initialize trial.
            List<Alternative> alternatives = new List<Alternative>();
            
            for(int i = 0; i < 10; ++i)
            {
                alternatives.Add(new Alternative()
                {
                    Text = "",
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
            _context.SaveChanges();
            return trial;
        }

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

        #region Getters 

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

        public CreateMapContentViewModel GetMapContentViewModel(String userName, String gameTitle)
        {
            Game game = GetGameAtTitle(userName, gameTitle);
            return new CreateMapContentViewModel()
            {
                GameTitle = game.Title,
                Map = SortMap(game.Map.ToList()),
                MapSize = game.TableSize
            };
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
