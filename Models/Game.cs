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
        public Game(Player player1, Player player2, int playUntilPoints)
        {
            Id = Guid.NewGuid().ToString();
            Player1 = player1;
            Player2 = player2;
            PlayUntilPoints = playUntilPoints;

            InitializeNewGame();
        }
        public string Id { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public List<Card> CardsPlayed { get; set; }
        public List<Card> CardsDrew { get; set; }
        public List<Card> Deck { get; set; }
        public int PlayUntilPoints { get; set; }
        public bool GameEnded { get; set; } = false;

        public bool MakeMove(string playerConnectionId, Card card)
        {
            var player = GetPlayerFromConnectionId(playerConnectionId);
            var opponent = GetOpponentFromConnectionId(playerConnectionId);

            if (playerConnectionId != _turnToPlay)
                return false;


            if (CardsPlayed.Count == 2)
            {
                CardsPlayed.Clear();
            }

            if (CardsDrew.Count == 2)
            {
                CardsDrew.Clear();
            }

            if (CardsPlayed.Count == 0)
            {
                CardsPlayed.Add(card);
                RemoveCardFromHand(player, card);
                ChangePlayersTurn();
            }
            else
            {
                var lastCardPlayed = CardsPlayed.FirstOrDefault();

                var isLastPoint = !Deck.Any() && !opponent.Cards.Any();

                if (card.Color != lastCardPlayed.Color)
                {
                    if (player.Cards.Any(x => x.Color == lastCardPlayed.Color))
                        return false;
                    opponent.Points += card.Value + lastCardPlayed.Value;
                    if (isLastPoint)
                    {
                        opponent.Points += 3;
                    }
                    ChangePlayersTurn();
                }
                else
                {
                    if (card.Number > lastCardPlayed.Number)
                    {
                        player.Points += card.Value + lastCardPlayed.Value;
                        if (isLastPoint)
                        {
                            player.Points += 3;
                        }
                    }
                    else
                    {
                        opponent.Points += card.Value + lastCardPlayed.Value;
                        if (isLastPoint)
                        {
                            opponent.Points += 3;
                        }
                        ChangePlayersTurn();
                    }
                }
                CardsPlayed.Add(card);
                RemoveCardFromHand(player, card);
                DrawCards();
            }
            return true;
        }

        // ------------------------------ private----------------------------------------------------//

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
            var cardToRemove = player.Cards.FirstOrDefault(x => x.Color == card.Color && x.Number == card.Number);
            player.Cards.Remove(cardToRemove);
        }


        private void DrawCards()
        {

            var roundEnded = DetectIfRoundEnded();
            if (roundEnded)
                return;

            var card1 = Deck.GetAndRemove(0, 1).First();
            var card2 = Deck.GetAndRemove(0, 1).First();
            Player1.Cards.Add(card1);
            Player2.Cards.Add(card2);
            CardsDrew.Add(card1);
            CardsDrew.Add(card2);
        }

        private bool DetectIfRoundEnded()
        {
            if (!Deck.Any() && !Player1.Cards.Any() && !Player2.Cards.Any())
            {
                Player1.CalculatedPoints += Player1.Points / 3;
                Player2.CalculatedPoints += Player2.Points / 3;

                if (Player1.CalculatedPoints >= PlayUntilPoints && Player2.CalculatedPoints >= PlayUntilPoints)
                {
                    PlayUntilPoints += 10;
                    InitializeNewGame();
                }
                else if (Player1.CalculatedPoints >= PlayUntilPoints || Player2.CalculatedPoints >= PlayUntilPoints)
                {
                    GameEnded = true;
                }
                else
                {
                    InitializeNewGame();
                }
                return true;
            }
            return false;
        }

        private void InitializeNewGame()
        {
            Deck = GenerateDeck();
            Player1.Cards = Deck.GetAndRemove(0, 10);
            Player2.Cards = Deck.GetAndRemove(0, 10);
            CardsPlayed = new List<Card>();
            CardsDrew = new List<Card>();
            _turnToPlay = Player1.ConnectionId; ;
        }

        private void ChangePlayersTurn()
        {
            _turnToPlay = Player1.ConnectionId == _turnToPlay ? Player2.ConnectionId : Player1.ConnectionId;
        }

    }
}