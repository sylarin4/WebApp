using System;
using System.Collections.Generic;

namespace AdventureGameEditor.Models.DatabaseModels.Game
{
    public class MapRow
    {
        public int ID { get; set; }
        public virtual ICollection<Field> Row { get; set; }
    }
}
