using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public interface IGameEditorService
    {
        public Boolean InicializeGame(String title, int mapSize, Visibility visibility);
    }
}
