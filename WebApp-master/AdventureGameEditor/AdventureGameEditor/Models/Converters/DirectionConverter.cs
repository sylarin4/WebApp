using System;

using AdventureGameEditor.Models.Enums;

namespace AdventureGameEditor.Models.Converters
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