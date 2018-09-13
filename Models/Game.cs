using System;
using System.Collections.Generic;
using System.Linq;
using TresetaApp.Enums;
using TresetaApp.Extensions;

namespace TresetaApp.Models
{
    public class Game
    {
        private string _turnToPlay;
        public Game(string player1ConnectionId, string player2ConnectionId)
        {
            Id = Guid.NewGuid().ToString();
            Deck = GenerateDeck();
            Player1 = new Player(Deck.GetAndRemove(0, 10), player1ConnectionId);
            Player2 = new Player(Deck.GetAndRemove(0, 10), player2ConnectionId);
            LastCardPlayed = null;
            _turnToPlay = player1ConnectionId;
        }
        public string Id { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public Card LastCardPlayed { get; set; }
        public List<Card> Deck { get; set; }
        public bool GameEnded { get; set; } = false;

        public bool MakeMove(string playerConnectionId, Card card)
        {
            var player = GetPlayerFromConnectionId(playerConnectionId);
            var opponent = GetOpponentFromConnectionId(playerConnectionId);

            if (playerConnectionId != _turnToPlay)
                return false;

            if (LastCardPlayed == null)
            {
                LastCardPlayed = card;
                RemoveCardFromHand(player, card);
                ChangePlayersTurn();
            }
            else
            {
                if (card.Color != LastCardPlayed.Color)
                {
                    if (player.Cards.Any(x => x.Color == LastCardPlayed.Color))
                        return false;
                    opponent.Points += card.Value + LastCardPlayed.Value;
                }
                else
                {
                    if (card.Number > LastCardPlayed.Number)
                    {
                        player.Points += card.Value + LastCardPlayed.Value;
                    }
                    else
                    {
                        opponent.Points += card.Value + LastCardPlayed.Value;
                        ChangePlayersTurn();
                    }
                }
                LastCardPlayed = null;
                RemoveCardFromHand(player, card);
                DrawCards();
            }
            return true;
        }

        // ------------------------------ private

        private List<Card> GenerateDeck()
        {
            var cards = new List<Card>();
            var allCardNumbers = (CardNumber[])Enum.GetValues(typeof(CardNumber));
            for (int i = 0; i < allCardNumbers.Length; i++)
            {
                var allCardColors = (CardColor[])Enum.GetValues(typeof(CardColor));
                for (int j = 0; j < allCardColors.Length; j++)
                {
                    var card = new Card(allCardColors[j], allCardNumbers[i]);
                    cards.Add(card);
                }

            }
            cards.Shuffle();
            return cards;
        }

        private Player GetPlayerFromConnectionId(string playerConnectionId)
        {
            if (Player1.ConnectionId == playerConnectionId)
                return Player1;
            return Player2;
        }

        private Player GetOpponentFromConnectionId(string playerConnectionId)
        {
            if (Player1.ConnectionId == playerConnectionId)
                return Player2;
            return Player1;
        }

        private void RemoveCardFromHand(Player player, Card card)
        {
            player.Cards.Remove(card);
        }


        private void DrawCards()
        {
            if (!Deck.Any())
            {
                if (!Player1.Cards.Any() && !Player2.Cards.Any())
                    GameEnded = true;
                return;
            }
            Player1.Cards.AddRange(Deck.GetAndRemove(0, 1));
            Player2.Cards.AddRange(Deck.GetAndRemove(0, 1));
        }


        private void ChangePlayersTurn()
        {
            _turnToPlay = Player1.ConnectionId == _turnToPlay ? Player2.ConnectionId : Player1.ConnectionId;
        }
    }

}