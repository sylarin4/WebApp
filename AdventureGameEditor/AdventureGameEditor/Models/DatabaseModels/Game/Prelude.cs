using System;

namespace AdventureGameEditor.Models.DatabaseModels.Game
{
    public class Prelude
    {
        public int ID { get; set; }
        public String Text { get; set; }
        public String GameTitle { get; set; }
        public User Owner { get; set; }
        public Image? Image { get; set; }
    }
}
