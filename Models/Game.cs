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
        private CardAndUser _firstCardPlayedInRound;
        public Game(GameSetup gameSetup)
        {
            CardsPlayed = new List<CardAndUser>();
            CardsDrew = new List<CardAndUser>();
            Teams = new List<Team>();
            Spectators = new List<User>();
            Players = new List<Player>();
            CardsPlayedPreviousRound = new List<CardAndUser>();
            ExcludedCards = new List<Card>();
            GameSetup = gameSetup;
        }
        public List<Player> Players { get; set; }
        public List<User> Spectators { get; set; }
        public List<Team> Teams { get; set; }
        public User UserTurnToPlay { get; set; }
        public List<CardAndUser> CardsPlayed { get; set; }
        public List<CardAndUser> CardsPlayedPreviousRound { get; set; }
        public List<CardAndUser> CardsDrew { get; set; }
        public List<Card> Deck { get; set; }
        public List<Card> ExcludedCards {get; set; }
        public bool GameEnded { get; set; } = false;
        public bool GameStarted { get; set; } = false;
        public bool IsFirstRound { get; set; } = false;
        public bool RoundEnded { get; set; } = false;
        public GameSetup GameSetup { get; }

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

            if (CardsPlayed.Count == Players.Count - 1)
            {
                CardsPlayedPreviousRound = CardsPlayed.ToList();
            }

            if (CardsDrew.Count == Players.Count)
            {
                CardsDrew.Clear();
            }

            if (CardsPlayed.Count != 0 && card.Color != _firstCardPlayedInRound.Card.Color && player.Cards.Any(x => x.Color == _firstCardPlayedInRound.Card.Color))
            {
                return false;
            }


            CardsPlayed.Add(new CardAndUser(card, player.User));

            if (CardsPlayed.Count == Players.Count)
            {
                CardsPlayedPreviousRound = CardsPlayed.ToList();
            }


            _firstCardPlayedInRound = CardsPlayed.FirstOrDefault();
            _strongestCardInRound = CardsPlayed.Where(x => x.Card.Color == _firstCardPlayedInRound.Card.Color).OrderByDescending(item => item.Card.Strength).First();
            RemoveCardFromHand(player, card);
            ChangePlayersTurn();


            if (CardsPlayed.Count == Players.Count)
            {
                var isLastPoint = Players.Where(x => x.Cards.Any()).Count() == 0;

                UserTurnToPlay = Players.FirstOrDefault(x => x.User.Name == _strongestCardInRound.User.Name).User;
                var teamRoundWinner = Teams.FirstOrDefault(x => x.Users.FirstOrDefault(y => y.ConnectionId == UserTurnToPlay.ConnectionId) != null);

                foreach (var cardPlayed in CardsPlayed)
                {
                    teamRoundWinner.Points += cardPlayed.Card.Value(GameSetup.GameMode);
                }

                if (isLastPoint)
                {
                    teamRoundWinner.Points += 3;
                    if(GameSetup.GameMode == GameMode.Evasion){
                        var remainingPoints = 0;
                        foreach (var team in Teams){
                            if(team.Name != teamRoundWinner.Name)
                                remainingPoints += team.Points % 3;
                        }

                        // Cappotto
                        if(Teams.All(x => x.Name == teamRoundWinner.Name || x.Points < 3)){
                            var total = teamRoundWinner.Points;
                            foreach(var team in Teams)
                                team.Points = total;
                            teamRoundWinner.Points = 0;
                        }
                    }
                }

                DrawCards();
            }
            if (CardsPlayed.Count == Players.Count && IsFirstRound)
                IsFirstRound = false;
            return true;
        }


        public bool StartGame()
        {
            if (Players.Count != GameSetup.ExpectedNumberOfPlayers)
                return false;

            if(GameSetup.GameMode == GameMode.Evasion){
                foreach(var player in Players)
                    Teams.Add(new Team(new List<User>{player.User}));
            }else{
                Teams.Add(new Team(Players.Where((c, i) => i % 2 == 0).Select(x => x.User).ToList()));
                Teams.Add(new Team(Players.Where((c, i) => i % 2 == 1).Select(x => x.User).ToList()));
            }
            
            UserTurnToPlay = Players.First().User;

            GameStarted = true;

            InitializeNewGame();

            return true;

        }

        public bool AddExtraPoints(string connectionId, List<Card> cards)
        {
            if (cards.Count != 3 && cards.Count != 4)
                return false;
            if (cards.Any(x => x.Number != CardNumber.Ace && x.Number != CardNumber.Two && x.Number != CardNumber.Three))
                return false;            
            if (GameSetup.GameMode == GameMode.Evasion)
                return false;
            
            var firstCardSample = cards[0];
            var secondCardSample = cards[1];
            var player = GetPlayerFromConnectionId(connectionId);
            ExtraPoint extraPoint;
            if (cards.Any(x => x.Color != firstCardSample.Color))
            {
                //3 or 4 of a kind
                if (cards.Any(x => x.Number != firstCardSample.Number))
                    return false;

                if (player.ExtraPoints.Any(x => x.Cards.Any(c =>
                 c.Number == firstCardSample.Number &&
                 c.Color == firstCardSample.Color &&
                 x.TypeOfExtraPoint == TypeOfExtraPoint.SameKind)))
                    return false;
                if (player.ExtraPoints.Any(x => x.Cards.Any(c =>
                 c.Number == secondCardSample.Number &&
                 c.Color == secondCardSample.Color &&
                 x.TypeOfExtraPoint == TypeOfExtraPoint.SameKind)))
                    return false;
                extraPoint = new ExtraPoint(cards, TypeOfExtraPoint.SameKind);
            }
            else
            {
                //napolitana
                if (player.ExtraPoints.Any(x => x.Cards.Any(c => c.Color == firstCardSample.Color && x.TypeOfExtraPoint == TypeOfExtraPoint.Napoletana)))
                    return false;
                extraPoint = new ExtraPoint(cards, TypeOfExtraPoint.Napoletana);
            }
            player.ExtraPoints.Add(extraPoint);
            var team = Teams.FirstOrDefault(x => x.Users.FirstOrDefault(y => y.ConnectionId == connectionId) != null);
            team.CalculatedPoints += cards.Count;
            return true;
        }


        public void InitializeNewGame()
        {
            Deck = GenerateDeck();
            ExcludedCards.Clear();

            var cardsPerPlayer = 10;
            var excludedCards = 0;

            if(GameSetup.GameMode == GameMode.Evasion){
                cardsPerPlayer = Deck.Count/Players.Count;
                excludedCards = Deck.Count % Players.Count;
            }

            foreach (var player in Players)
            {
                player.Cards = Deck.GetAndRemove(0, cardsPerPlayer);
                player.ExtraPoints.Clear();
            }
            ExcludedCards = Deck.GetAndRemove(0, excludedCards);

            foreach (var team in Teams)
            {
                team.Points = 0;
            }
            CardsPlayed.Clear();
            CardsDrew.Clear();
            CardsPlayedPreviousRound.Clear();
            RoundEnded = false;
            IsFirstRound = true;
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
                    var card = new Card(allCardColors[j], allCardNumbers[i],GameSetup.TypeOfDeck);
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

                var allPlayersExceeded = Teams.Where(y => y.CalculatedPoints < GameSetup.PlayUntilPoints).Count() == 0;
                var atLeastOnePlayerExceeded = Teams.Where(y => y.CalculatedPoints >= GameSetup.PlayUntilPoints).Count() > 0;

                if (allPlayersExceeded)
                {
                    GameSetup.PlayUntilPoints += 10;
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