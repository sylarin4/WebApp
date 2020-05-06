using AdventureGameEditor.Models.Enums;

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
