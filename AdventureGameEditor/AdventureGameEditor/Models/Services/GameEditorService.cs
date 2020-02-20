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
            Trace.WriteLine(title);
            Trace.WriteLine(mapSize);
            Trace.WriteLine(visibility);
            Trace.WriteLine(owner.UserName);
            _context.Game.Add(
                new Game
                {
                    Title = title,
                    Visibility = visibility,
                    TableSize = mapSize,
                    PlayCounter = 0,
                    Owner = owner,
                    Map = new List<Field>()
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
                Map = game.Map.ToList(),
                GameTitle = game.Title
            };
        }

        public void AddTextToAFieldAt(String userName, String gameTitle, int rowNumber, int colNumber, String text)
        {
            Game game = GetGameAtTitle(userName, gameTitle);
            Boolean fieldIsAlreadyExists = false;
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
            _context.SaveChanges();
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
