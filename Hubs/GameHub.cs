using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TresetaApp.Models;

namespace TresetaApp.Hubs
{
    public class GameHub : Hub
    {
        private static List<Game> _games = new List<Game>();
        private static List<User> _users = new List<User>();
        private static List<WaitingRoom> _waitingRooms = new List<WaitingRoom>();

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user != null)
                _users.Remove(user);

       
            await CleanupUserFromWaitingRooms();
            await CleanupUserFromGames();
            await CleanupUserFromUsersList();

            await GetAllPlayers();
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
            else
            {
                waitingRoom.Users.Add(user);
                await UpdateSingleWaitingRoom(waitingRoom);
                await AllWaitingRoomsUpdate();
            }
        }

        public async Task ExitGame(string gameid)
        {
            var game = _games.SingleOrDefault(x => x.Id == gameid);

            var allConnectionIdsFromTheGame = game.Players.Select(y => y.User.ConnectionId).ToList();
            await Clients.Clients(allConnectionIdsFromTheGame).SendAsync("ExitGame");
            _games.Remove(game);
        }

        public async Task AddNewMessage(string message)
        {
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            var msg = new ChatMessage(user.Name, message);
            await Clients.All.SendAsync("AddNewMessage", msg);
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
                await AllWaitingRoomsUpdate();
            }
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

            var game = new Game(players, waitingRoom.PlayUntilPoints);
            _games.Add(game);
            var allConnectionIdsFromTheGame = game.Players.Select(y => y.User.ConnectionId).ToList();
            await Clients.Clients(allConnectionIdsFromTheGame).SendAsync("GameStarted", game);
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

        // private

        private async Task RemoveWaitingRoom(string id)
        {
            _waitingRooms.Remove(_waitingRooms.FirstOrDefault(x => x.Id == id));
            await AllWaitingRoomsUpdate();
        }

        private async Task GameUpdated(Game game)
        {
            var allConnectionIdsFromTheGame = game.Players.Select(y => y.User.ConnectionId).ToList();
            await Clients.Clients(allConnectionIdsFromTheGame).SendAsync("GameUpdate", game);
        }



        private async Task UpdateSingleWaitingRoom(WaitingRoom waitingRoom)
        {
            var allConnectionIds = waitingRoom.Users.Select(x => x.ConnectionId).ToList();
            await Clients.Clients(allConnectionIds).SendAsync("SingleWaitingRoomUpdate", waitingRoom);
        }

        private async Task CleanupUserFromWaitingRooms()
        {
            _waitingRooms.ForEach(async y =>
            {
                if (y.Users.Any(x => x.ConnectionId == Context.ConnectionId))
                {
                    await LeaveWaitingRoom(y.Id);
                }
            });
        }

        private async Task CleanupUserFromGames()
        {
            List<Game> games = _games.Where(x => x.Players.Where(y => y.User.ConnectionId == Context.ConnectionId).Count() > 0).ToList();

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
        }

    }
}