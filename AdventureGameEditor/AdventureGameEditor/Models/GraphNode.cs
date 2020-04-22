using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class GraphNode
    {
        // The column index of the field which represented by this node.
        public int ColNumber { get; set; }

        // The row index of the field which represented by this node.
        public int RowNumber { get; set; }

        // The node that's before this node in the path.
        public GraphNode Parent { get; set; }

        // The length of the way from the start node to this node.
        public int PathLength { get; set; }

        // Represents if we can go in direction up from the field which represented by this node.
        public Boolean IsUpWay { get; set; }
        public Boolean IsDownWay { get; set; }
        public Boolean IsRightWay { get; set; }
        public Boolean IsLeftWay { get; set; }

    }
}
