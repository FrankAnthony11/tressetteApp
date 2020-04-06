using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using TresetaApp.Enums;
using TresetaApp.Models;
using TresetaApp.Models.Dtos;

namespace TresetaApp.Hubs
{
    public class GameHub : Hub
    {
        private static List<Game> _games = new List<Game>();
        private static List<User> _users = new List<User>();
        private readonly IMapper _mapper;

        public GameHub(IMapper mapper)
        {
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user == null)
                return;

            await SendMessageToAllChat("Server", $"{user.Name} has left the server.", TypeOfMessage.Server);

            await CleanupUserFromGames();
            await CleanupUserFromUsersList();

            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddUser(string name)
        {

            User user;
            lock (_users)
            {
                name = Regex.Replace(name, @"\s+", "").ToLower();

                if (name.Length > 10)
                    name = name.Substring(0, 10);

                var nameExists = _users.Any(x => x.Name == name);
                if (nameExists)
                {
                    Random rnd = new Random();
                    name = name + rnd.Next(1, 100);
                }


                user = new User(Context.ConnectionId, name);

                _users.Add(user);
            }


            await GetAllPlayers();
            await Clients.Client(Context.ConnectionId).SendAsync("GetCurrentUser", user);
            await SendMessageToAllChat("Server", $"{user.Name} has connected to the server.", TypeOfMessage.Server);
            await base.OnConnectedAsync();
        }

        public async Task GetAllPlayers()
        {
            await Clients.All.SendAsync("GetAllPlayers", _users);
        }


        public async Task SetGamePassword(string id, string password)
        {
            var game = _games.FirstOrDefault(x => x.GameSetup.Id == id);
            if (game == null)
                return;
            game.GameSetup.Password = password;
            await UpdateAllGames();
            await DisplayToastMessageToGame(id, "Password updated");
        }

        public async Task SetGameTypeOfDeck(string id, int typeOfDeck)
        {
            var game = _games.FirstOrDefault(x => x.GameSetup.Id == id);
            if (game == null)
                return;
            game.GameSetup.TypeOfDeck = (TypeOfDeck)typeOfDeck;
            await UpdateAllGames();
            await DisplayToastMessageToGame(id, "Type of Deck updated");
        }

        public async Task UpdateAllGames()
        {
            var games = _mapper.Map<List<GameDto>>(_games);
            await Clients.All.SendAsync("UpdateAllGames", games);
        }

        public async Task CreateGame(int playUntilPoints, int expectedNumberOfPlayers, GameMode gameMode)
        {

            await CleanupUserFromGames();

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            var gameSetup = new GameSetup(playUntilPoints, expectedNumberOfPlayers,gameMode);
            
            var game = new Game(gameSetup);
            game.Players.Add(new Player(user));
            _games.Add(game);
            await GameUpdated(game);
            await UpdateAllGames();
            await SendMessageToAllChat("Server", $"User {user.Name} has created new game", TypeOfMessage.Server);
        }


        public async Task CallAction(string action, string gameid)
        {
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            var message = $"Player {user.Name} is calling on action: {action}";
            await DisplayToastMessageToGame(gameid, message);
            if(action == "KNOCK")
                await Clients.All.SendAsync("KnockPlayer");
            else if(action == "SLIDE"){
                //TODO
            }
        }


        public async Task ExitGame(string gameid)
        {
            var game = _games.SingleOrDefault(x => x.GameSetup.Id == gameid);

            if (game == null)
                return;

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            var allSpectatorsFromTheGame = GetSpectatorsFromGame(game);
            var allPlayersFromGame = GetPlayersFromGame(game);

            if (allSpectatorsFromTheGame.Contains(Context.ConnectionId))
            {
                game.Spectators.Remove(game.Spectators.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId));
            }

            if (allPlayersFromGame.Contains(Context.ConnectionId))
            {
                var player = game.Players.FirstOrDefault(y => y.User.ConnectionId == Context.ConnectionId);
                if (game.GameStarted)
                {
                    player.LeftGame = true;
                    await DisplayToastMessageToGame(gameid, $"USER {player.User.Name} HAS LEFT THE GAME.");
                }
                else
                {
                    game.Players.Remove(player);
                }
            }


            if (!game.Players.Any(x => x.LeftGame == false) && !game.Spectators.Any())
                _games.Remove(game);

            await GameUpdated(game);
            await UpdateAllGames();
            await SendMessageToGameChat(gameid, "Server", $"{user.Name} has left the game.", TypeOfMessage.Server);
        }

        public async Task SendMessageToAllChat(string username, string message, TypeOfMessage typeOfMessage = TypeOfMessage.Chat)
        {

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            Regex regex = new Regex(@"^/(slap|buzz|alert) ([A-Za-z0-9\s]*)$");
            Match match = regex.Match(message);

            if (match.Success)
            {
                var targetedUsername = match.Groups[2].Value;
                var targetedUser = _users.FirstOrDefault(x => x.Name == targetedUsername);
                if (targetedUser != null)
                {
                    await Clients.Client(targetedUser.ConnectionId).SendAsync("BuzzPlayer");
                    await SendMessageToAllChat("Server", $"User {user.Name} has buzzed player {targetedUser.Name} ", TypeOfMessage.Server);
                }
                else
                {
                    await SendMessageToAllChat("Server", $"Player not found", TypeOfMessage.Server);
                }
                return;
            }

            var msg = new ChatMessage(username, message, typeOfMessage);
            await Clients.All.SendAsync("SendMessageToAllChat", msg);
        }

        public async Task SendMessageToGameChat(string gameId, string username, string message, TypeOfMessage typeOfMessage = TypeOfMessage.Chat)
        {

            var game = _games.FirstOrDefault(x => x.GameSetup.Id == gameId);
            if (game == null)
                return;

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user == null)
                return;

            Regex regex = new Regex(@"^/(slap|buzz|alert) ([A-Za-z0-9\s]*)$");
            Match match = regex.Match(message);

            if (match.Success)
            {
                var targetedUsername = match.Groups[2].Value;
                var targetedUser = _users.FirstOrDefault(x => x.Name == targetedUsername);

                if (targetedUser != null)
                {
                    await Clients.Client(targetedUser.ConnectionId).SendAsync("BuzzPlayer");
                    await SendMessageToGameChat(gameId, "Server", $"User {user.Name} has buzzed player {targetedUser.Name} ", TypeOfMessage.Server);
                }
                else
                {
                    await SendMessageToGameChat(gameId, "Server", $"Player not found", TypeOfMessage.Server);
                }
                return;

            }

            var msg = new ChatMessage(username, message, typeOfMessage);

            var usersToNotify = GetPlayersFromGame(game);
            usersToNotify.AddRange(GetSpectatorsFromGame(game));

            await Clients.Clients(usersToNotify).SendAsync("SendMessageToGameChat", msg);
        }

        public async Task KickUserFromGame(string connectionId, string gameId)
        {
            var game = _games.FirstOrDefault(x => x.GameSetup.Id == gameId);
            if (game == null) return;

            var user = _users.FirstOrDefault(x => x.ConnectionId == connectionId);
            if (user == null) return;

            game.Players.Remove(game.Players.SingleOrDefault(y => y.User.ConnectionId == connectionId));

            await GameUpdated(game);
            await UpdateAllGames();
            await Clients.Client(connectionId).SendAsync("KickUSerFromGame");
        }

        public async Task StartGame(string gameId)
        {
            var game = _games.FirstOrDefault(x => x.GameSetup.Id == gameId);
            if (game == null) return;

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user == null) return;

            var success = game.StartGame();

            if (success)
                await GameUpdated(game);

            await UpdateAllGames();


        }
        public async Task JoinGame(string gameId, string password)
        {

            await CleanupUserFromGamesExceptThisGame(gameId);


            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user == null)
            {
                return;
            }


            var game = _games.SingleOrDefault(x => x.GameSetup.Id == gameId);
            if (game == null)
                return;

            var isAlreadySpectator = game.Spectators.Contains(user);

            if (!string.IsNullOrEmpty(game.GameSetup.Password) && !isAlreadySpectator)
                if (game.GameSetup.Password != password)
                    return;

            if (!game.GameStarted)
            {
                if (isAlreadySpectator)
                {
                    //join the gamt that hasn't started
                    if (game.Players.Count == game.GameSetup.ExpectedNumberOfPlayers)
                        return;
                    game.Spectators.Remove(user);
                    game.Players.Add(new Player(user));
                }
                else
                {
                    //spectate game that hasn't started
                    game.Spectators.Add(user);
                    await SendMessageToGameChat(gameId, "Server", $"{user.Name} has joined the game room.", TypeOfMessage.Server);
                }
            }
            else
            {
                var playerLeftWithThisNickname = game.Players.FirstOrDefault(x => x.LeftGame && x.User.Name == user.Name);

                if (playerLeftWithThisNickname != null)
                {
                    playerLeftWithThisNickname.User = user;
                    playerLeftWithThisNickname.LeftGame = false;

                    game.Teams.ForEach(t =>
                    {
                        t.Users.ForEach(u =>
                        {
                            if (u.Name == user.Name)
                            {
                                u.ConnectionId = user.ConnectionId;
                            }
                        });
                    });
                    if (game.UserTurnToPlay.Name == user.Name)
                        game.UserTurnToPlay = user;
                    await DisplayToastMessageToGame(gameId, $"PLAYER {user.Name} HAS RECONNECTED TO THE GAME");
                    await SendMessageToGameChat(gameId, "Server", $"{user.Name} has joined the game room.", TypeOfMessage.Server);
                }
                else
                {
                    game.Spectators.Add(user);
                    await SendMessageToGameChat(gameId, "Server", $"{user.Name} has joined the game room.", TypeOfMessage.Server);
                }
            }


            await GameUpdated(game);
            await UpdateAllGames();
        }


        public async Task MakeMove(string gameId, Card card)
        {
            var game = _games.SingleOrDefault(x => x.GameSetup.Id == gameId);
            lock (game)
            {
                if (game == null)
                    return;
                if (game.GameEnded)
                    return;
                var success = game.MakeMove(Context.ConnectionId, card);
                if (!success)
                    return;
            }
            await GameUpdated(game);

        }

        public async Task StartNewRound(string gameId)
        {
            var game = _games.SingleOrDefault(x => x.GameSetup.Id == gameId);
            if (game == null)
                return;
            game.InitializeNewGame();
            await GameUpdated(game);
        }

        public async Task AddExtraPoints(string gameId, List<Card> cards)
        {
            var game = _games.SingleOrDefault(x => x.GameSetup.Id == gameId);
            if (game == null)
                return;
            if (game.GameEnded)
                return;

            var success = game.AddExtraPoints(Context.ConnectionId, cards);
            if (!success)
            {
                await DisplayToastMessageToUser(Context.ConnectionId, "You cannot add these extra points");
                return;
            }
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            var message = $"Player {user.Name} added extra points:";
            cards.ForEach(x =>
            {
                message += $"{(CardColor)x.Color} {(CardNumber)x.Number},";
            });
            await DisplayToastMessageToGame(gameId, message);

            var usersToNotify = GetPlayersFromGame(game);
            usersToNotify.AddRange(GetSpectatorsFromGame(game));


            await SendMessageToGameChat(gameId, "Server", $"Player {user.Name} has added {cards.Count} extra points.", TypeOfMessage.Server);
            await GameUpdated(game);
        }

        // -------------------------------private--------------------


        private async Task DisplayToastMessageToGame(string gameid, string message)
        {
            var game = _games.FirstOrDefault(x => x.GameSetup.Id == gameid);
            if (game == null)
                return;
            var usersToNotify = GetPlayersFromGame(game);
            usersToNotify.AddRange(GetSpectatorsFromGame(game));

            await Clients.Clients(usersToNotify).SendAsync("DisplayToastMessage", message);
        }
        private async Task DisplayToastMessageToUser(string connectionId, string message)
        {
            await Clients.Client(connectionId).SendAsync("DisplayToastMessage", message);
        }




        private async Task GameUpdated(Game game)
        {
            var allPlayersInTheGame = GetPlayersFromGame(game);
            var gameDto = _mapper.Map<GameDto>(game);

            var allSpectatorsInTheGame = GetSpectatorsFromGame(game);
            await Clients.Clients(allSpectatorsInTheGame).SendAsync("GameUpdate", gameDto);

            if (game.GameStarted)
            {
                foreach (var connectionId in allPlayersInTheGame)
                {
                    gameDto.MyCards = game.Players.FirstOrDefault(x => x.User.ConnectionId == connectionId).Cards;
                    await Clients.Client(connectionId).SendAsync("GameUpdate", gameDto);
                }
            }
            else
            {
                await Clients.Clients(allPlayersInTheGame).SendAsync("GameUpdate", gameDto);
            }

        }


        private List<string> GetPlayersFromGame(Game game)
        {
            return game.Players.Where(x => !x.LeftGame).Select(y => y.User.ConnectionId).ToList();
        }

        private List<string> GetSpectatorsFromGame(Game game)
        {
            return game.Spectators.Select(y => y.ConnectionId).ToList();
        }


        private async Task CleanupUserFromGames()
        {
            List<Game> games = _games.Where(x => GetPlayersFromGame(x).Where(y => y == Context.ConnectionId).Any()).ToList();

            games.AddRange(_games.Where(x => GetSpectatorsFromGame(x).Where(y => y == Context.ConnectionId).Any()).ToList());

            foreach (var game in games)
            {
                await ExitGame(game.GameSetup.Id);
            }
        }

        private async Task CleanupUserFromGamesExceptThisGame(string gameId)
        {
            List<Game> games = _games.Where(x => x.GameSetup.Id != gameId && GetPlayersFromGame(x).Where(y => y == Context.ConnectionId).Any()).ToList();

            games.AddRange(_games.Where(x => x.GameSetup.Id != gameId && GetSpectatorsFromGame(x).Where(y => y == Context.ConnectionId).Any()).ToList());

            foreach (var game in games)
            {
                await ExitGame(game.GameSetup.Id);
            }
        }


        private async Task CleanupUserFromUsersList()
        {
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (user != null)
            {
                _users.Remove(user);
            }
            await GetAllPlayers();
        }
    }
}