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
        public Boolean InicializeGame(String title, int mapSize, Visibility visibility)
        {
            _context.Game.Any(game => game.Title == title);
            return true;
        }
    }
}
