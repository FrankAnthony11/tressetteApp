using System;
using System.Collections.Generic;
using System.Linq;
using TresetaApp.Enums;
using TresetaApp.Extensions;
using TresetaApp.Models;

namespace TresetaApp.Models
{
    public class Game
    {
        private CardAndUser _strongestCardInRound;
        private CardAndUser _firstCardPlayed;
        public Game(string id, List<Player> players, int playUntilPoints)
        {
            Id = id;
            Players = players;
            PlayUntilPoints = playUntilPoints;

            InitializeNewGame();
        }
        public string Id { get; set; }
        public List<Player> Players { get; set; }
        public User UserTurnToPlay { get; set; }
        public List<CardAndUser> CardsPlayed { get; set; }
        public List<CardAndUser> CardsDrew { get; set; }
        public List<Card> Deck { get; set; }
        public int PlayUntilPoints { get; set; }
        public bool GameEnded { get; set; } = false;

        public bool MakeMove(string playerConnectionId, Card card)
        {
            var player = GetPlayerFromConnectionId(playerConnectionId);

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

            if (CardsPlayed.Count != 0 && card.Color != _firstCardPlayed.Card.Color && player.Cards.Any(x => x.Color == _firstCardPlayed.Card.Color))
            {
                return false;
            }

            CardsPlayed.Add(new CardAndUser(card, player.User));
            _firstCardPlayed = CardsPlayed.FirstOrDefault();
            _strongestCardInRound = CardsPlayed.Where(x => x.Card.Color == _firstCardPlayed.Card.Color).OrderByDescending(item => item.Card.Strength).First();
            RemoveCardFromHand(player, card);
            ChangePlayersTurn();


            if (CardsPlayed.Count == Players.Count)
            {
                var isLastPoint = Players.Where(x => x.Cards.Any()).Count() == 0;

                UserTurnToPlay = _strongestCardInRound.User;
                var roundWinner = Players.FirstOrDefault(x => x.User.ConnectionId == UserTurnToPlay.ConnectionId);

                foreach (var cardPlayed in CardsPlayed)
                {
                    roundWinner.Points += cardPlayed.Card.Value;
                }

                if (isLastPoint)
                {
                    roundWinner.Points += 3;
                }
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

        private Player GetNextPlayerFromConnectionId(string playerConnectionId)
        {
            var currentPlayerToPlay = GetPlayerFromConnectionId(playerConnectionId);

            var index = Players.IndexOf(currentPlayerToPlay);

            if (index == Players.Count - 1)
            {
                return Players.First();
            }
            else
            {
                return Players[index + 1];
            }
        }

        private void RemoveCardFromHand(Player player, Card card)
        {
            var cardToRemove = player.Cards.FirstOrDefault(x => x.Color == card.Color && x.Number == card.Number);
            player.Cards.Remove(cardToRemove);
        }


        private void DrawCards()
        {

            var gameEnded = DetectIfGameEnded();
            if (gameEnded)
                return;

            if (Deck.Any())
            {
                foreach (var player in Players)
                {
                    var card = Deck.GetAndRemove(0, 1).First();
                    player.Cards.Add(card);
                    CardsDrew.Add(new CardAndUser(card,player.User));
                }
            }
        }

        private bool DetectIfGameEnded()
        {
            if (Players.Where(x => x.Cards.Any()).Count() == 0)
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
                    //game has ended
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
            CardsPlayed = new List<CardAndUser>();
            CardsDrew = new List<CardAndUser>();
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