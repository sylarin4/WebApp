﻿using System;

namespace AdventureGameEditor.Models.ViewModels.Gameplay
{
    public class DirectionButtonsViewModel
    {
        public String GameTitle { get; set; }
        public int RowNumber { get; set; }
        public int ColNumber { get; set; }
        // If this time, the player can't go on his/her own, cause the
        // game will teleport him/her to a field.
        public Boolean WillTeleport { get; set; }
        public Boolean GameLost { get; set; }
        public Boolean GameWon { get; set; }
        public Boolean IsUpWay { get; set; }
        public Boolean IsDownWay { get; set; }
        public Boolean IsRightWay { get; set; }
        public Boolean IsLeftWay { get; set; }
    }
}
