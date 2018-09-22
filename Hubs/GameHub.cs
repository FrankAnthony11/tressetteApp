using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using TresetaApp.Models;
using TresetaApp.Models.Dtos;

namespace TresetaApp.Hubs
{
    public class GameHub : Hub
    {
        private static List<Game> _games = new List<Game>();
        private static List<User> _users = new List<User>();
        private static List<WaitingRoom> _waitingRooms = new List<WaitingRoom>();
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


            await CleanupUserFromWaitingRooms();
            await CleanupUserFromGames();
            await CleanupUserFromUsersList();


            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddUser(string name)
        {
            var user = new User(Context.ConnectionId, name);

            _users.Add(user);
            await GetAllPlayers();
            await Clients.Client(Context.ConnectionId).SendAsync("GetCurrentUser", user);
            await base.OnConnectedAsync();
        }

        public async Task GetAllPlayers()
        {
            await Clients.All.SendAsync("GetAllPlayers", _users);
        }

        public async Task AllWaitingRoomsUpdate()
        {
            await Clients.All.SendAsync("AllWaitingRoomsUpdate", _waitingRooms.Where(y => y.Users.Count < y.ExpectedNumberOfPlayers));
        }

        public async Task CreateWaitingRoom(int playUntilPoints, int expectedNumberOfPlayers)
        {
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            var waitingRoom = new WaitingRoom(user, playUntilPoints, expectedNumberOfPlayers);
            _waitingRooms.Add(waitingRoom);
            await UpdateSingleWaitingRoom(waitingRoom);
            await AllWaitingRoomsUpdate();
        }

        public async Task JoinWaitingRoom(string id)
        {

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            var waitingRoom = _waitingRooms.FirstOrDefault(x => x.Id == id);

            if (waitingRoom == null)
            {
                return;
            }

            waitingRoom.Users.Add(user);
            await UpdateSingleWaitingRoom(waitingRoom);
            await AllWaitingRoomsUpdate();
        }

        public async Task CallAction(string action, string gameid)
        {
            var game = _games.SingleOrDefault(x => x.Id == gameid);
            var allConnectionIds = GetConnectionIdsFromGame(game);

            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

            var message = $"Player {user.Name} is calling on action: {action}";

            await Clients.Clients(allConnectionIds).SendAsync("DisplayToastMessage", message);
        }
        public async Task ExitGame(string gameid)
        {
            var game = _games.SingleOrDefault(x => x.Id == gameid);

            var allConnectionIdsFromTheGame = GetConnectionIdsFromGame(game);
            await Clients.Clients(allConnectionIdsFromTheGame).SendAsync("ExitGame");
            _games.Remove(game);
        }

        public async Task AddNewMessageToAllChat(string message)
        {
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            var msg = new ChatMessage(user, message);
            await Clients.All.SendAsync("AddNewMessageToAllChat", msg);
        }

        public async Task AddNewMessageToGameChat(string gameOrWaitingRoomId, string message)
        {
            //todo
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            var msg = new ChatMessage(user, message);

            var allConnectionIds = new List<string>();

            var game = _games.FirstOrDefault(x => x.Id == gameOrWaitingRoomId);
            if (game != null)
            {
                allConnectionIds = GetConnectionIdsFromGame(game);
            }
            else
            {
                var waitingRoom = _waitingRooms.FirstOrDefault(x => x.Id == gameOrWaitingRoomId);
                allConnectionIds = waitingRoom.Users.Select(y => y.ConnectionId).ToList();
            }
            await Clients.Clients(allConnectionIds).SendAsync("AddNewMessageToGameChat", msg);
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
            await AllWaitingRoomsUpdate();
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
            await GameUpdated(game);
            await RemoveWaitingRoom(waitingRoomId);
        }

        public async Task MakeMove(string gameId, Card card)
        {
            var game = _games.SingleOrDefault(x => x.Id == gameId);
            if (game.GameEnded)
                return;
            var success = game.MakeMove(Context.ConnectionId, card);
            if (!success)
                return;
            await GameUpdated(game);
        }

        // -------------------------------private--------------------

        private async Task RemoveWaitingRoom(string id)
        {
            _waitingRooms.Remove(_waitingRooms.FirstOrDefault(x => x.Id == id));
            await AllWaitingRoomsUpdate();
        }

        private async Task GameUpdated(Game game)
        {
            var allConnectionIdsFromTheGame = GetConnectionIdsFromGame(game);

            var gameDto = _mapper.Map<GameDto>(game);

            foreach (var connectionId in allConnectionIdsFromTheGame)
            {
                gameDto.MyCards = game.Players.FirstOrDefault(x => x.User.ConnectionId == connectionId).Cards;
                await Clients.Client(connectionId).SendAsync("GameUpdate", gameDto);
            }
        }

        private List<string> GetConnectionIdsFromGame(Game game)
        {
            return game.Players.Select(y => y.User.ConnectionId).ToList();
        }

        private async Task UpdateSingleWaitingRoom(WaitingRoom waitingRoom)
        {
            var allConnectionIds = waitingRoom.Users.Select(x => x.ConnectionId).ToList();
            await Clients.Clients(allConnectionIds).SendAsync("SingleWaitingRoomUpdate", waitingRoom);
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
            List<Game> games = _games.Where(x => GetConnectionIdsFromGame(x).Where(y => y == Context.ConnectionId).Any()).ToList();

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