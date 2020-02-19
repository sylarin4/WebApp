using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

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
                    Owner = owner
                }
                );
            _context.SaveChanges();
            return true;
        }
    }
}
