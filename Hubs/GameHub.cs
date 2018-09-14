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
        private static List<string> _players = new List<string>();
        private static List<WaitingRoom> _waitingRooms = new List<WaitingRoom>();

        public override async Task OnConnectedAsync()
        {
            _players.Add(Context.ConnectionId);
            await GetAllPlayers();
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            _players.Remove(Context.ConnectionId);

            await CleanupPlayerFromWaitingRooms();
            await GetAllPlayers();
            await base.OnDisconnectedAsync(exception);
        }



        public async Task GetAllPlayers()
        {
            await Clients.All.SendAsync("GetAllPlayers", _players);
        }

        public async Task AllWaitingRoomsUpdate()
        {
            await Clients.All.SendAsync("AllWaitingRoomsUpdate", _waitingRooms.Where(y => string.IsNullOrEmpty(y.ConnectionId2)));
        }

        public async Task CreateWaitingRoom()
        {
            var waitingRoom = new WaitingRoom(Context.ConnectionId);
            _waitingRooms.Add(waitingRoom);
            await UpdateSingleWaitingRoom(waitingRoom);
            await AllWaitingRoomsUpdate();
        }

        public async Task JoinWaitingRoom(string id)
        {
           // await CleanupPlayerFromWaitingRooms();

            var waitingRoom = _waitingRooms.FirstOrDefault(x => x.Id == id);

            if (waitingRoom == null)
            {
                await CreateWaitingRoom();
            }
            else
            {
                waitingRoom.ConnectionId2 = Context.ConnectionId;
                await UpdateSingleWaitingRoom(waitingRoom);
                await AllWaitingRoomsUpdate();
            }
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
                await UpdateSingleWaitingRoom(waitingRoom);
                await AllWaitingRoomsUpdate();
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

        private async Task UpdateSingleWaitingRoom(WaitingRoom waitingRoom)
        {
            await Clients.Clients(waitingRoom.ConnectionId1, waitingRoom.ConnectionId2).SendAsync("SingleWaitingRoomUpdate", waitingRoom);
        }

        private async Task CleanupPlayerFromWaitingRooms()
        {
            _waitingRooms.ForEach(async y =>
            {
                if (y.ConnectionId1 == Context.ConnectionId || y.ConnectionId2 == Context.ConnectionId)
                {
                    await LeaveWaitingRoom(y.Id);
                }
            });
        }

    }
}