﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models
{
    public class GameDetailsViewModel
    {
        [Display(Name = "A játék készítője")]
        public String OwnerName { get; set; }

        [Display(Name = "Cím")]
        public String Title { get; set; }

        // Stores who can see and play this game.

        [Display(Name = "Láthatóság")]
        public Visibility Visibility { get; set; }

        // Stores the size of the map.
        [Display(Name = "Térkép mérete")]
        public int TableSize { get; set; }

        // Stores the map of the game.
        [Display(Name = "Térkép")]
        public virtual ICollection<MapRow> Map { get; set; }

        // Stores on which field the game starts.
        [Display(Name = "Kezdőmező")]
        public Field StartField { get; set; }

        // Stores the player should reach which field to win the game.
        [Display(Name = "Célmező")]
        public Field TargetField { get; set; }
    }
}