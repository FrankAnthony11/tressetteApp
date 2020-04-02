using TresetaApp.Enums;
using System;

namespace TresetaApp.Models
{
    public class Card
    {
        public Card(CardColor color, CardNumber number, TypeOfDeck typeOfDeck)
        {
            Color = color;
            Number = number;
            ImageUrl = $"/images/{(int)typeOfDeck}/{(int)color}/{(int)number}.jpg";
        }
        public string ImageUrl { get; set; }
        public CardColor Color { get; set; }
        public CardNumber Number { get; set; }

        public int Value(GameMode gameMode)
        {
            switch (Number)
            {
                case CardNumber.Ace:
                    if(gameMode == GameMode.Evasion && Color == CardColor.Bastoni){
                        return 15;
                    }
                    else
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
        public int Strength
        {
            get { return (int)Number; }
        }



    }
}