using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventureGameEditor.Models
{
    public class MapImage
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
        public MapTheme Theme { get; set; }
        public int WayDirectionsCode { get; set; }
    }
}
