using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public static class DirectionConverter
    {
        public static Direction StringToDirection(String directionString)
        {
            switch (directionString)
            {
                case "Up":
                    return Direction.Up;
                case "Down":
                    return Direction.Down;
                case "Right":
                    return Direction.Right;
                case "Left":
                    return Direction.Left;
                default:
                    return Direction.NotSet;
            }
        }

    }
}
