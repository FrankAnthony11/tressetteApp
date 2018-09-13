using System;
using System.Collections.Generic;
using System.Linq;
using TresetaApp.Enums;
using TresetaApp.Extensions;

namespace TresetaApp.Models
{
    public class Game
    {
        public Game()
        {
            Deck = GenerateDeck();
            Player1 = new Player(Deck.GetAndRemove(0,10));
            Player2 = new Player(Deck.GetAndRemove(0,10));
        }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public List<Card> Deck { get; set; }
        public bool GameStarted { get; set; } = false;
        public bool GameEnded { get; set; } = false;

        private List<Card> GenerateDeck()
        {
            var cards = new List<Card>();
            var allCardNumbers = (CardNumber[])Enum.GetValues(typeof(CardNumber));
            for (int i = 0; i < allCardNumbers.Length; i++)
            {
                var allCardColors = (CardColor[])Enum.GetValues(typeof(CardColor));
                for (int j = 0; j < allCardColors.Length; j++)
                {
                    var card = new Card()
                    {
                        Number = allCardNumbers[i],
                        Color = allCardColors[j],
                        Value = GetCardValue(allCardNumbers[i]),
                        Strength = (int)allCardNumbers[i]

                    };
                    cards.Add(card);
                }

            }
            cards.Shuffle();
            return cards;
        }

        private int GetCardValue(CardNumber cardNumber)
        {
            switch (cardNumber)
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

}