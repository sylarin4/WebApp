using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    // Represents which directions we can leave a field. 
    // Which direction is in the name of and enum item that means there's a way in that direction.
    // We enumerate the direction's name in order: Up, Right, Down, Left.
    public enum WayDirectionsViewModel
    {
        Empty,
        Up,
        Right,
        Down,
        Left,
        UpRight,
        UpDown,
        UpLeft,
        RightDown,
        RightLeft,
        DownLeft,
        UpRightLeft,
        UpDownLeft,
        UpRightDown,
        RightDownLeft,
        UpRightDownLeft
    }
}
