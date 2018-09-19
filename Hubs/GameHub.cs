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
            await GetAllPlayers();
            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddUser(string name)
        {

            await CleanupUserFromWaitingRooms();
            await CleanupUserFromGames();
            await CleanupUserFromUsersList();
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
            await Clients.All.SendAsync("AllWaitingRoomsUpdate", _waitingRooms.Where(y => y.User2 == null));
        }

        public async Task CreateWaitingRoom(int playUntilPoints)
        {
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            var waitingRoom = new WaitingRoom(user, playUntilPoints);
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
                waitingRoom.User2 = user; ;
                await UpdateSingleWaitingRoom(waitingRoom);
                await AllWaitingRoomsUpdate();
            }
        }

        public async Task ExitGame(string gameid)
        {
            var game = _games.SingleOrDefault(x => x.Id == gameid);
            await Clients.Clients(game.Player1.User.ConnectionId, game.Player2.User.ConnectionId).SendAsync("ExitGame");
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

            if (waitingRoom.User1 == user)
            {
                waitingRoom.User1 = waitingRoom.User2;
            } 
            waitingRoom.User2 = null;

            if (waitingRoom.User1 == null)
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

            if (waitingRoom.User1 == null || waitingRoom.User2 == null)
                return;

            var game = new Game(new Player(waitingRoom.User1), new Player(waitingRoom.User2), waitingRoom.PlayUntilPoints);
            _games.Add(game);
            await Clients.Clients(game.Player1.User.ConnectionId, game.Player2.User.ConnectionId).SendAsync("GameStarted", game);
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
            await Clients.Clients(game.Player1.User.ConnectionId, game.Player2.User.ConnectionId).SendAsync("GameUpdate", game);
        }



        private async Task UpdateSingleWaitingRoom(WaitingRoom waitingRoom)
        {
            await Clients.Clients(waitingRoom.User1.ConnectionId, waitingRoom.User2 == null ? "" : waitingRoom.User2.ConnectionId).SendAsync("SingleWaitingRoomUpdate", waitingRoom);
        }

        private async Task CleanupUserFromWaitingRooms()
        {
            _waitingRooms.ForEach(async y =>
            {
                if (y.User1 != null)
                {
                    if (y.User1.ConnectionId == Context.ConnectionId)
                    {
                        await LeaveWaitingRoom(y.Id);
                    }
                }
                if (y.User2 != null)
                {
                    if (y.User2.ConnectionId == Context.ConnectionId)
                    {
                        await LeaveWaitingRoom(y.Id);
                    }
                }
            });
        }

        private async Task CleanupUserFromGames()
        {
            List<Game> games = _games.Where(x => x.Player1.User.ConnectionId == Context.ConnectionId || (x.Player2.User != null && x.Player2.User.ConnectionId == Context.ConnectionId)).ToList();

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