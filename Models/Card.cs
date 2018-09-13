using TresetaApp.Enums;

namespace TresetaApp.Models
{
    public class Card
    {
        public Card(CardColor color, CardNumber number)
        {
            Color = color;
            Number = number;
        }
        public string ImageUrl { get; set; }
        public CardColor Color { get; set; }
        public CardNumber Number { get; set; }
        public int Value
        {
            get
            {
                switch (Number)
                {
                    case CardNumber.Ace:
                        return 3;
                    case CardNumber.Two:
                    case CardNumber.Three:
                    case CardNumber.Fante:
                    case CardNumber.Cavallo:
                    case CardNumber.Re:
                        return 1;
                    default:
                        return 0;
                }
            }
        }
        public int Strength
        {
            get { return (int)Number; }
        }



    }
}