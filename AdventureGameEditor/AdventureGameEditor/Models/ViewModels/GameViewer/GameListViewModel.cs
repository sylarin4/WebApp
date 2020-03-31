using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class GameListViewModel
    {
        public List<Game> GameList { get; set; }
        public OrderType OrderType { get; set; }
    }

    public enum OrderType
    {
        Alphabetical,
        Popularity
    }
}
