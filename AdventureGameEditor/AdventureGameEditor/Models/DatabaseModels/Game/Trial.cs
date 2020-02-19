using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class Trial
    {
        public int ID { get; set; }

        // What can be happens during this trial. (The options.)
        public virtual ICollection<Alternative> Alternatives { get; set; }

        // Stores if it's a luck trial or multiple choice.
        public TrialType TrialType { get; set; }
    }
}
