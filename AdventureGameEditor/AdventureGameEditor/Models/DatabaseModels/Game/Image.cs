using System;

namespace AdventureGameEditor.Models.DatabaseModels.Game
{
    public class Image
    {
        public int ID { get; set; }
        public String Name { get; set; }
        public byte[] Picture { get; set; }
    }
}
