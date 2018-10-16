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
        private static List<GameSetup> _waitingRooms = new List<GameSetup>();
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
            await Clients.Client(user.ConnectionId).SendAsync("AddNewMessageToAllChat", new ChatMessage("Server", $"{user.Name} has left the server.", TypeOfMessage.Server));


            await CleanupUserFromWaitingRooms();
            await CleanupUserFromGames();
            await CleanupUserFromUsersList();


            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddUser(string name)
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


            var user = new User(Context.ConnectionId, name);

            _users.Add(user);
            await GetAllPlayers();
            await Clients.Client(Context.ConnectionId).SendAsync("GetCurrentUser", user);
            await Clients.Client(user.ConnectionId).SendAsync("AddNewMessageToAllChat", new ChatMessage("Server", $"{user.Name} has connected to the server.", TypeOfMessage.Server));
            await base.OnConnectedAsync();
        }

        public async Task GetAllPlayers()
        {
            await Clients.All.SendAsync("GetAllPlayers", _users);
        }


        public async Task SetRoomPassword(string id, string password)
        {
            var waitingRoom = _waitingRooms.FirstOrDefault(x => x.Id == id);
            if (waitingRoom == null)
                return;
            waitingRoom.Password = password;
            await UpdateSingleWaitingRoom(waitingRoom);
            await UpdateAllWaitingRooms();
            await DisplayToastMessage(waitingRoom.Users.Select(x => x.ConnectionId).ToList(), "Password updated");
        }

        public async Task UpdateAllWaitingRooms()
        {
            await Clients.All.SendAsync("UpdateAllWaitingRooms", _waitingRooms.Where(y => y.Users.Count < y.ExpectedNumberOfPlayers));
        }
        public async Task UpdateAllRunningGames()
        {
            await Clients.All.SendAsync("UpdateAllRunningGames", _mapper.Map<List<GameDto>>(_games));
        }

        public async Task CreateWaitingRoom(int playUntilPoints, int expectedNumberOfPlayers)
        {
            await CleanupUserFromWaitingRooms();

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            var waitingRoom = new GameSetup(user, playUntilPoints, expectedNumberOfPlayers);
            _waitingRooms.Add(waitingRoom);
            await UpdateSingleWaitingRoom(waitingRoom);
            await UpdateAllWaitingRooms();
            await AddNewMessageToAllChat($"User {user.Name} has created new game", TypeOfMessage.Server);
        }

        public async Task JoinWaitingRoom(string id, string password)
        {

            var rooms = _waitingRooms.Where(x => x.Users.FirstOrDefault(y => y.ConnectionId == Context.ConnectionId) != null).ToList();

            if (rooms.FirstOrDefault(x => x.Id == id) != null)
                return;

            await CleanupUserFromGames();
            await CleanupUserFromWaitingRooms();

            var waitingRoom = _waitingRooms.FirstOrDefault(x => x.Id == id);

            if (waitingRoom == null) return;

            if (!string.IsNullOrEmpty(waitingRoom.Password) && waitingRoom.Password != password)
            {
                await DisplayToastMessage(Context.ConnectionId, "Incorrect password");
                return;
            }

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            waitingRoom.Users.Add(user);
            await UpdateSingleWaitingRoom(waitingRoom);
            await UpdateAllWaitingRooms();
        }

        public async Task CallAction(string action, string gameid)
        {
            var game = _games.SingleOrDefault(x => x.Id == gameid);
            var allConnectionIds = GetPlayersConnectionIdsFromTheGame(game);

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            var message = $"Player {user.Name} is calling on action: {action}";

            await DisplayToastMessage(allConnectionIds, message);

        }


        public async Task ExitGame(string gameid)
        {
            var game = _games.SingleOrDefault(x => x.Id == gameid);

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (game == null)
                return;
            var allSpectatorsFromTheGame = GetSpectatorsConnectionIdsFromTheGame(game);

            if (allSpectatorsFromTheGame.Contains(Context.ConnectionId))
            {
                game.Spectators.Remove(game.Spectators.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId));
            }
            else
            {

                var allPlayersFromTheGame = GetPlayersConnectionIdsFromTheGame(game);

                var player = game.Players.FirstOrDefault(y => y.User.ConnectionId == Context.ConnectionId);
                player.LeftGame = true;


                await DisplayToastMessage(allPlayersFromTheGame, $"USER {player.User.Name} HAS LEFT THE GAME.");
                await DisplayToastMessage(allSpectatorsFromTheGame, $"USER {player.User.Name} HAS LEFT THE GAME.");

                await GameUpdatedToPlayers(game);
                await GameUpdatedToSpectators(game);

                if (!game.Players.Any(x => x.LeftGame == false))
                    _games.Remove(game);
                await UpdateAllRunningGames();
            }
            await AddNewMessageToGameChat(gameid, $"{user.Name} has left the game room.", TypeOfMessage.Server);


        }

        public async Task AddNewMessageToAllChat(string message, TypeOfMessage typeOfMessage = TypeOfMessage.Chat)
        {

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            Regex regex = new Regex(@"^/(slap|buzz|alert) ([A-Za-z0-9\s]*)$");
            Match match = regex.Match(message);

            if (match.Success)
            {
                var username = match.Groups[2].Value;
                var targetedUser = _users.FirstOrDefault(x => x.Name == username);
                if (targetedUser != null)
                {
                    await Clients.Client(targetedUser.ConnectionId).SendAsync("BuzzPlayer");
                    await Clients.Client(user.ConnectionId).SendAsync("AddNewMessageToAllChat", new ChatMessage("Server", $"User {user.Name} has buzzed player {targetedUser.Name} ", TypeOfMessage.Server));

                }
                else
                {
                    await Clients.Client(user.ConnectionId).SendAsync("AddNewMessageToAllChat", new ChatMessage("Server", "Player not found", TypeOfMessage.Server));
                }
                return;
            }

            var msg = new ChatMessage(user.Name, message, typeOfMessage);
            await Clients.All.SendAsync("AddNewMessageToAllChat", msg);
        }

        public async Task AddNewMessageToGameChat(string gameOrWaitingRoomId, string message, TypeOfMessage typeOfMessage = TypeOfMessage.Chat)
        {

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            Regex regex = new Regex(@"^/(slap|buzz|alert) ([A-Za-z0-9\s]*)$");
            Match match = regex.Match(message);

            if (match.Success)
            {
                var username = match.Groups[2].Value;
                var targetedUser = _users.FirstOrDefault(x => x.Name == username);

                if (targetedUser != null)
                {
                    await Clients.Client(targetedUser.ConnectionId).SendAsync("BuzzPlayer");
                    await AddNewMessageToGameChat(gameOrWaitingRoomId, $"User {user.Name} has buzzed player {targetedUser.Name} ", TypeOfMessage.Server);
                }
                else
                {
                    await Clients.Client(user.ConnectionId).SendAsync("AddNewMessageToGameChat", new ChatMessage("Server", "Player not found", TypeOfMessage.Server));
                }
                return;

            }

            var msg = new ChatMessage(user.Name, message, typeOfMessage);
            var allConnectionIds = new List<string>();

            var game = _games.FirstOrDefault(x => x.Id == gameOrWaitingRoomId);
            if (game != null)
            {
                allConnectionIds = GetPlayersConnectionIdsFromTheGame(game);
                allConnectionIds.AddRange(GetSpectatorsConnectionIdsFromTheGame(game));
            }
            else
            {
                var waitingRoom = _waitingRooms.FirstOrDefault(x => x.Id == gameOrWaitingRoomId);
                if (waitingRoom != null)
                    allConnectionIds = waitingRoom.Users.Select(y => y.ConnectionId).ToList();
            }
            await Clients.Clients(allConnectionIds).SendAsync("AddNewMessageToGameChat", msg);
        }

        public async Task KickUserFromWaitingRoom(string connectionId, string waitingroomId)
        {
            var waitingRoom = _waitingRooms.FirstOrDefault(x => x.Id == waitingroomId);
            if (waitingRoom == null) return;

            var user = _users.FirstOrDefault(x => x.ConnectionId == connectionId);
            if (user == null) return;

            waitingRoom.Users.Remove(user);

            await UpdateSingleWaitingRoom(waitingRoom);
            await UpdateAllWaitingRooms();
            await Clients.Client(connectionId).SendAsync("KickUserFromWaitingRoom");
        }



        public async Task LeaveWaitingRoom(string id)
        {
            var waitingRoom = _waitingRooms.FirstOrDefault(x => x.Id == id);

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            if (waitingRoom.Users.Contains(user))
            {
                waitingRoom.Users.Remove(user);
            }

            if (waitingRoom.Users.Count == 0)
            {
                await RemoveWaitingRoom(id);
            }
            else
            {
                await UpdateSingleWaitingRoom(waitingRoom);
            }
            await UpdateAllWaitingRooms();
        }

        public async Task CreateGame(string waitingRoomId)
        {
            var waitingRoom = _waitingRooms.FirstOrDefault(x => x.Id == waitingRoomId);

            if (waitingRoom.Users.Count != waitingRoom.ExpectedNumberOfPlayers)
                return;


            var players = new List<Player>();

            waitingRoom.Users.ForEach(x =>
             {
                 players.Add(new Player(x));

             });

            var game = new Game(waitingRoom.Id, players, waitingRoom.PlayUntilPoints);
            _games.Add(game);
            await GameUpdatedToPlayers(game);
            await RemoveWaitingRoom(waitingRoomId);
            await UpdateAllRunningGames();
        }

        public async Task JoinGameAsPlayerOrSpectator(string gameId)
        {

            List<Game> games = _games.Where(x => GetPlayersConnectionIdsFromTheGame(x).Where(y => y == Context.ConnectionId).Any()).ToList();

            if (games.FirstOrDefault(x => x.Id == gameId) != null)
                return;

            await CleanupUserFromGames();
            await CleanupUserFromWaitingRooms();


            var game = _games.SingleOrDefault(x => x.Id == gameId);
            if (game == null)
                return;
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user == null)
            {
                return;
            }

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
                await DisplayToastMessage(GetPlayersConnectionIdsFromTheGame(game), $"PLAYER {user.Name} HAS RECONNECTED TO THE GAME");
                await DisplayToastMessage(GetSpectatorsConnectionIdsFromTheGame(game), $"PLAYER {user.Name} HAS RECONNECTED TO THE GAME");
                await GameUpdatedToPlayers(game);
            }
            else
            {
                game.Spectators.Add(user);
                await GameUpdatedToSpectators(game);
            }
            await AddNewMessageToGameChat(gameId, $"{user.Name} has joined the game room.", TypeOfMessage.Server);
        }

        public async Task MakeMove(string gameId, Card card)
        {
            var game = _games.SingleOrDefault(x => x.Id == gameId);
            if (game == null)
                return;
            if (game.GameEnded)
                return;
            var success = game.MakeMove(Context.ConnectionId, card);
            if (!success)
                return;
            await GameUpdatedToPlayers(game);
            await GameUpdatedToSpectators(game);
        }

        public async Task StartNewRound(string gameId)
        {
            var game = _games.SingleOrDefault(x => x.Id == gameId);
            if (game == null)
                return;
            game.InitializeNewGame();
            await GameUpdatedToPlayers(game);
        }

        public async Task AddExtraPoints(string gameId, List<Card> cards)
        {

            var game = _games.SingleOrDefault(x => x.Id == gameId);
            if (game == null)
                return;
            if (game.GameEnded)
                return;
            var success = game.AddExtraPoints(Context.ConnectionId, cards);
            if (!success)
            {
                await DisplayToastMessage(Context.ConnectionId, "You cannot add these extra points");
                return;
            }
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            var message = $"Player {user.Name} added extra points:";
            cards.ForEach(x =>
            {
                message += $"{(CardColor)x.Color} {(CardNumber)x.Number},";
            });
            await DisplayToastMessage(GetPlayersConnectionIdsFromTheGame(game), message);
            await DisplayToastMessage(GetSpectatorsConnectionIdsFromTheGame(game), message);
            await Clients.Clients(GetPlayersConnectionIdsFromTheGame(game)).SendAsync("AddNewMessageToGameChat", new ChatMessage("Server", $"Player {user.Name} has added {cards.Count} extra points.", TypeOfMessage.Server));
            await Clients.Clients(GetSpectatorsConnectionIdsFromTheGame(game)).SendAsync("AddNewMessageToGameChat", new ChatMessage("Server", $"Player {user.Name} has added {cards.Count} extra points.", TypeOfMessage.Server));
            await GameUpdatedToPlayers(game);
            await GameUpdatedToSpectators(game);
        }

        // -------------------------------private--------------------


        private async Task DisplayToastMessage(List<string> allConnectionIds, string message)
        {
            await Clients.Clients(allConnectionIds).SendAsync("DisplayToastMessage", message);
        }
        private async Task DisplayToastMessage(string connectionId, string message)
        {
            await Clients.Client(connectionId).SendAsync("DisplayToastMessage", message);
        }

        private async Task RemoveWaitingRoom(string id)
        {
            _waitingRooms.Remove(_waitingRooms.FirstOrDefault(x => x.Id == id));
            await UpdateAllWaitingRooms();
        }

        private async Task GameUpdatedToSpectators(Game game)
        {


            var allSpectatorsInTheGame = GetSpectatorsConnectionIdsFromTheGame(game);
            var gameDto = _mapper.Map<GameDto>(game);
            await Clients.Clients(allSpectatorsInTheGame).SendAsync("GameUpdate", gameDto);


        }

        private async Task GameUpdatedToPlayers(Game game)
        {

            var allPlayersInTheGame = GetPlayersConnectionIdsFromTheGame(game);
            var gameDto = _mapper.Map<GameDto>(game);
            foreach (var connectionId in allPlayersInTheGame)
            {
                gameDto.MyCards = game.Players.FirstOrDefault(x => x.User.ConnectionId == connectionId).Cards;
                await Clients.Client(connectionId).SendAsync("GameUpdate", gameDto);
            }
        }


        private List<string> GetPlayersConnectionIdsFromTheGame(Game game)
        {
            return game.Players.Where(x => !x.LeftGame).Select(y => y.User.ConnectionId).ToList();
        }

        private List<string> GetSpectatorsConnectionIdsFromTheGame(Game game)
        {
            return game.Spectators.Select(y => y.ConnectionId).ToList();
        }

        private async Task UpdateSingleWaitingRoom(GameSetup waitingRoom)
        {
            var allConnectionIds = waitingRoom.Users.Select(x => x.ConnectionId).ToList();
            await Clients.Clients(allConnectionIds).SendAsync("UpdateSingleWaitingRoom", waitingRoom);
        }

        private async Task CleanupUserFromWaitingRooms()
        {
            var rooms = _waitingRooms.Where(x => x.Users.FirstOrDefault(y => y.ConnectionId == Context.ConnectionId) != null).ToList();
            foreach (var waitingRoom in rooms)
            {
                await LeaveWaitingRoom(waitingRoom.Id);
            }
        }

        private async Task CleanupUserFromGames()
        {
            List<Game> games = _games.Where(x => GetPlayersConnectionIdsFromTheGame(x).Where(y => y == Context.ConnectionId).Any()).ToList();

            games.AddRange(_games.Where(x => GetSpectatorsConnectionIdsFromTheGame(x).Where(y => y == Context.ConnectionId).Any()).ToList());

            foreach (var game in games)
            {
                await ExitGame(game.Id);
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