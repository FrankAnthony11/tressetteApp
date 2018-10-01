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

            Teams = new List<Team>();
            Teams.Add(new Team(Players.Where((c, i) => i % 2 == 0).Select(x => x.User).ToList()));
            Teams.Add(new Team(Players.Where((c, i) => i % 2 == 1).Select(x => x.User).ToList()));

            Spectators = new List<User>();

            UserTurnToPlay = Players.First().User;

            InitializeNewGame();
        }
        public string Id { get; set; }
        public List<Player> Players { get; set; }
        public List<User> Spectators { get; set; }
        public List<Team> Teams { get; set; }
        public User UserTurnToPlay { get; set; }
        public List<CardAndUser> CardsPlayed { get; set; }
        public List<CardAndUser> CardsDrew { get; set; }
        public List<Card> Deck { get; set; }
        public int PlayUntilPoints { get; set; }
        public bool GameEnded { get; set; } = false;
        public bool RoundEnded { get; set; } = false;

        public bool MakeMove(string playerConnectionId, Card card)
        {
            var player = GetPlayerFromConnectionId(playerConnectionId);

            if (player.Cards.FirstOrDefault(x => x.Color == card.Color && x.Number == card.Number) == null)
                return false;

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

                UserTurnToPlay = Players.FirstOrDefault(x => x.User.Name == _strongestCardInRound.User.Name).User;
                var teamRoundWinner = Teams.FirstOrDefault(x => x.Users.FirstOrDefault(y => y.ConnectionId == UserTurnToPlay.ConnectionId) != null);

                foreach (var cardPlayed in CardsPlayed)
                {

                    teamRoundWinner.Points += cardPlayed.Card.Value;
                }

                if (isLastPoint)
                {
                    teamRoundWinner.Points += 3;
                }
                DrawCards();
            }
            return true;
        }


        public void InitializeNewGame()
        {
            Deck = GenerateDeck();
            foreach (var player in Players)
            {
                player.Cards = Deck.GetAndRemove(0, 10);
            }
            foreach (var team in Teams)
            {
                team.Points = 0;
            }
            CardsPlayed = new List<CardAndUser>();
            CardsDrew = new List<CardAndUser>();
            RoundEnded = false;
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

            DetectIfGameOrRoundEnded();


            if (Deck.Any())
            {
                foreach (var player in Players)
                {
                    var card = Deck.GetAndRemove(0, 1).First();
                    player.Cards.Add(card);
                    CardsDrew.Add(new CardAndUser(card, player.User));
                }
            }
        }

        private void DetectIfGameOrRoundEnded()
        {
            if (Players.Where(x => x.Cards.Any()).Count() == 0)
            {
                RoundEnded = true;

                foreach (var team in Teams)
                {
                    team.CalculatedPoints += team.Points / 3;
                }

                var allPlayersExceeded = Teams.Where(y => y.CalculatedPoints < PlayUntilPoints).Count() == 0;
                var atLeastOnePlayerExceeded = Teams.Where(y => y.CalculatedPoints >= PlayUntilPoints).Count() > 0;

                if (allPlayersExceeded)
                {
                    PlayUntilPoints += 10;
                }
                else if (atLeastOnePlayerExceeded)
                {
                    GameEnded = true;
                }
            }
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