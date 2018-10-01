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
            var waitingRoom = new WaitingRoom(user, playUntilPoints, expectedNumberOfPlayers);
            _waitingRooms.Add(waitingRoom);
            await UpdateSingleWaitingRoom(waitingRoom);
            await UpdateAllWaitingRooms();
        }

        public async Task JoinWaitingRoom(string id, string password)
        {
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

            if (game == null)
                return;
            var allSpectatorsFromTheGame = GetSpectatorsConnectionIdsFromTheGame(game);

            if (allSpectatorsFromTheGame.Contains(Context.ConnectionId)){
                game.Spectators.Remove(game.Spectators.FirstOrDefault(x=>x.ConnectionId==Context.ConnectionId));
                return;
            }
            var allPlayersFromTheGame = GetPlayersConnectionIdsFromTheGame(game);
            await Clients.Clients(allPlayersFromTheGame).SendAsync("ExitGame");
            await Clients.Clients(allSpectatorsFromTheGame).SendAsync("ExitGame");
            _games.Remove(game);
            await UpdateAllRunningGames();

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
                allConnectionIds = GetPlayersConnectionIdsFromTheGame(game);
                allConnectionIds.AddRange(GetSpectatorsConnectionIdsFromTheGame(game));
            }
            else
            {
                var waitingRoom = _waitingRooms.FirstOrDefault(x => x.Id == gameOrWaitingRoomId);
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

        public async Task JoinGameAsSpectator(string gameId)
        {
            var game = _games.SingleOrDefault(x => x.Id == gameId);
            if (game == null)
                return;
            var user = _users.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user == null)
            {
                return;
            }
            game.Spectators.Add(user);
            await GameUpdatedToSpectators(game);
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
            return game.Players.Select(y => y.User.ConnectionId).ToList();
        }

        private List<string> GetSpectatorsConnectionIdsFromTheGame(Game game)
        {
            return game.Spectators.Select(y => y.ConnectionId).ToList();
        }

        private async Task UpdateSingleWaitingRoom(WaitingRoom waitingRoom)
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