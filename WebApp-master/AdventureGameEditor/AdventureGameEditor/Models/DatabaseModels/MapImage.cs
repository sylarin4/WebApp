using AdventureGameEditor.Models.Enums;

namespace AdventureGameEditor.Models.DatabaseModels
{
    public class MapImage
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
        public MapTheme Theme { get; set; }
        public int WayDirectionsCode { get; set; }
    }
}
