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
            /*Boolean fieldIsAlreadyExists = false;
            foreach(Field field in game.Map)
            {
                if(field.HorizontalCord == rowNumber && field.VerticalCord == colNumber)
                {
                    field.Text = text;
                    fieldIsAlreadyExists = true;
                }
            }
            if (!fieldIsAlreadyExists)
            {
                game.Map.Add(
                    new Field
                    {
                        HorizontalCord = rowNumber,
                        VerticalCord = colNumber,
                        Text = text
                    });
            }
            _context.SaveChanges();*/
        }


        #region Helper functions

        private Game GetGameAtTitle(String userName, String gameTitle)
        {
            return _context.Game.Where(g => g.Owner.UserName == userName && g.Title == gameTitle)
                .Include(g => g.Map)
                .FirstOrDefault();
        }

        #endregion
    }
}
