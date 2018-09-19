using System;
using System.Collections.Generic;
using System.Linq;
using TresetaApp.Enums;
using TresetaApp.Extensions;

namespace TresetaApp.Models
{
    public class Game
    {
        public Game(List<Player> players, int playUntilPoints)
        {
            Id = Guid.NewGuid().ToString();
            Players = players;
            PlayUntilPoints = playUntilPoints;

            InitializeNewGame();
        }
        public string Id { get; set; }
        public List<Player> Players { get; set; }
        public User UserTurnToPlay { get; set; }
        public List<Card> CardsPlayed { get; set; }
        public List<Card> CardsDrew { get; set; }
        public List<Card> Deck { get; set; }
        public int PlayUntilPoints { get; set; }
        public bool GameEnded { get; set; } = false;

        public bool MakeMove(string playerConnectionId, Card card)
        {
            var player = GetPlayerFromConnectionId(playerConnectionId);
            var opponent = GetOpponentFromConnectionId(playerConnectionId);

            if (playerConnectionId != UserTurnToPlay.ConnectionId)
                return false;


            if (CardsPlayed.Count == Players.Count)
            {
                CardsPlayed.Clear();
            }

            if (CardsDrew.Count == Players.Count)
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
            return Players.SingleOrDefault(x => x.User.ConnectionId == playerConnectionId);
        }

        private Player GetOpponentFromConnectionId(string playerConnectionId)
        {
            return Players.SingleOrDefault(x => x.User.ConnectionId != playerConnectionId);
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

            if (Deck.Any())
            {
                foreach (var player in Players)
                {
                    var card = Deck.GetAndRemove(0, 1).First();
                    player.Cards.Add(card);
                    CardsDrew.Add(card);
                }
            }
        }

        private bool DetectIfRoundEnded()
        {
            if (!Deck.Any() && Players.Where(x => x.Cards.Any()).Count() == 0)
            {

                foreach (var player in Players)
                {
                    player.CalculatedPoints += player.Points / 3;
                }

                var allPlayersExceeded = Players.Where(y => y.CalculatedPoints < PlayUntilPoints).Count() == 0;

                if (allPlayersExceeded)
                {
                    PlayUntilPoints += 10;
                    InitializeNewGame();
                    return true;
                }

                var atLeastOnePlayerExceeded = Players.Where(y => y.CalculatedPoints >= PlayUntilPoints).Count() > 0;

                if (atLeastOnePlayerExceeded)
                {
                    GameEnded = true;
                    return true;
                }

                InitializeNewGame();
                return true;
            }
            return false;
        }

        private void InitializeNewGame()
        {
            Deck = GenerateDeck();
            foreach (var player in Players)
            {
                player.Cards = Deck.GetAndRemove(0, 10);
                player.Points = 0;
            }
            CardsPlayed = new List<Card>();
            CardsDrew = new List<Card>();
            UserTurnToPlay = Players.First().User; ;
        }

        private void ChangePlayersTurn()
        {
            var currentPlayerToPlay = Players.SingleOrDefault(x => x.User.ConnectionId == UserTurnToPlay.ConnectionId);

            var index = Players.IndexOf(currentPlayerToPlay);

            if (index == Players.Count - 1)
            {
                UserTurnToPlay = Players.First().User;
            }
            else
            {
                UserTurnToPlay = Players[index + 1].User;
            }

        }

    }
}