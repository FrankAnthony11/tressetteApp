using TresetaApp.Enums;

namespace TresetaApp.Models
{
    public class Card
    {
        public string ImageUrl { get; set; }
        public CardColor Color { get; set; }
        public CardNumber Number { get; set; }
        public int Value { get; set; }
        public int Strength { get; set; }
    }
}