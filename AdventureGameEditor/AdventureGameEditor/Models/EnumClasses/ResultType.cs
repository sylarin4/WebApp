﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AdventureGameEditor.Models
{
    // Enumerates the type of results of a trial. (Used in TrialResult class.)
    public enum ResultType
    {
        [Display(Name = "Teleportáció")]
        Teleport,
        [Display(Name = "Semmi")]
        Nothing,
        [Display(Name = "Játék elvesztése")]
        GameLost,
        [Display(Name ="Játék megnyerése")]
        GameWon
    }
}
