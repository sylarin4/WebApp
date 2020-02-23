using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

using AdventureGameEditor.Data;

namespace AdventureGameEditor.Models
{
    public class GameEditorService : IGameEditorService
    {
        protected readonly AdventureGameEditorContext _context;
        public GameEditorService(AdventureGameEditorContext context)
        {
            _context = context;
        }
        public Boolean InicializeGame(String title, int mapSize, Visibility visibility, User owner)
        {
            if (_context.Game.Any(game => game.Title == title && game.Owner == owner)) return false;
            
            // Test writing on console.
            Trace.WriteLine(title);
            Trace.WriteLine(mapSize);
            Trace.WriteLine(visibility);
            Trace.WriteLine(owner.UserName);

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
                            HorizontalCord = i,
                            VerticalCord = j,
                            Text = "Semmi.",
                            ExitRoads = new ExitRoads
                            {
                                IsRightWay = false,
                                IsLeftWay = false,
                                IsUpWay = false,
                                IsDownWay = false
                            }
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
                    Map = map
                }
                );
            _context.SaveChanges();
            return true;
        }

        public MapViewModel GetMapViewModel(String userName, String gameTitle)
        {
            Game game = GetGameAtTitle(userName, gameTitle);
            return new MapViewModel
            {
                MapSize = game.TableSize,
                GameTitle = game.Title,
                Map = game.Map.ToList()
            };
        }

        public void AddTextToAFieldAt(String userName, String gameTitle, int rowNumber, int colNumber, String text)
        {
            Game game = GetGameAtTitle(userName, gameTitle);
            // TODO: make it more effective.
            foreach(MapRow row in game.Map)
            {
                foreach(Field field in row.Row)
                {
                    if(field.HorizontalCord == rowNumber && field.VerticalCord == colNumber)
                    {
                        field.Text = text;
                    }
                }
            }
            _context.SaveChanges();
        }

        public void SetExitRoads(String userName, String gameTitle, int rowNumber, int colNumber, int exitRoadsCode)
        {
            Field field = GetFieldAtCoordinate(userName, gameTitle, rowNumber, colNumber);
            if(field != null)
            {
                // Set direction "up".
                if(exitRoadsCode % 10 == 0)
                    field.ExitRoads.IsUpWay = false;
                else
                    field.ExitRoads.IsUpWay = true;

                // Set direction "right".
                if((exitRoadsCode/ 10)%10 == 0)
                    field.ExitRoads.IsRightWay = false;
                else
                    field.ExitRoads.IsRightWay = true;

                // Set direction "down".
                if((exitRoadsCode/100)%10 == 0)
                    field.ExitRoads.IsDownWay = false;
                else
                    field.ExitRoads.IsDownWay = true;

                // Set direction "left".
                if((exitRoadsCode/1000)%10 == 0)
                    field.ExitRoads.IsLeftWay = false;
                else
                    field.ExitRoads.IsLeftWay = true;

                _context.SaveChanges();
            }
        }


        #region Helper functions

        private Game GetGameAtTitle(String userName, String gameTitle)
        {
            return _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                .Include(g => g.Map)
                .ThenInclude(map => map.Row)
                .FirstOrDefault();
        }

        private Field GetFieldAtCoordinate(String userName, String gameTitle, int rowNumber, int colNumber)
        {
            Game game = GetGameAtTitle(userName, gameTitle);
            foreach (MapRow row in game.Map)
            {
                foreach (Field field in row.Row)
                {
                    if (field.HorizontalCord == rowNumber && field.VerticalCord == colNumber)
                    {
                        return field;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
