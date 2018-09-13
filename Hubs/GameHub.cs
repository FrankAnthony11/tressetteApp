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
        private static List<string> _players = new List<string>();
        private static List<WaitingRoom> _waitingRooms = new List<WaitingRoom>();

        public override async Task OnConnectedAsync()
        {
            _players.Add(Context.ConnectionId);
            await Clients.All.SendAsync("PlayerConnected", _players);
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            _players.Remove(Context.ConnectionId);
            await Clients.All.SendAsync("PlayerDisconnected", _players);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task AllWaitingRoomsUpdate()
        {
            await Clients.All.SendAsync("AllWaitingRoomsUpdate", _waitingRooms);
        }
        public async Task CreateWaitingRoom()
        {
            var waitingRoom = new WaitingRoom(Context.ConnectionId);
            _waitingRooms.Add(waitingRoom);
            await AllWaitingRoomsUpdate();
        }

        public async Task JoinWaitingRoom(string id)
        {
            var waitingRoom = _waitingRooms.FirstOrDefault(x => x.Id == id);
            waitingRoom.ConnectionId2 = Context.ConnectionId;
            await Clients.Clients(waitingRoom.ConnectionId1, waitingRoom.ConnectionId2).SendAsync("SingleWaitingRoomUpdate", waitingRoom);
        }

        public async Task LeaveWaitingRoom(string id)
        {
            var waitingRoom = _waitingRooms.FirstOrDefault(x => x.Id == id);

            if (waitingRoom.ConnectionId1 == Context.ConnectionId)
            {
                waitingRoom.ConnectionId1 = waitingRoom.ConnectionId2;
            }
            waitingRoom.ConnectionId2 = string.Empty;

            if (string.IsNullOrEmpty(waitingRoom.ConnectionId1))
            {
                await RemoveWaitingRoom(id);
            }
            else
            {
                await Clients.Client(waitingRoom.ConnectionId1).SendAsync("SingleWaitingRoomUpdate", waitingRoom);
            }
        }

        public async Task CreateGame(string waitingRoomId)
        {
            var waitingRoom = _waitingRooms.FirstOrDefault(x => x.Id == waitingRoomId);

            if (string.IsNullOrEmpty(waitingRoom.ConnectionId1) || string.IsNullOrEmpty(waitingRoom.ConnectionId2))
                return;

            var game = new Game(waitingRoom.ConnectionId1, waitingRoom.ConnectionId2);
            _games.Add(game);
            await Clients.Clients(game.Player1.ConnectionId, game.Player2.ConnectionId).SendAsync("GameUpdate", game);
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
            await Clients.Clients(game.Player1.ConnectionId, game.Player2.ConnectionId).SendAsync("GameUpdate", game);
        }

    }
}