using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace UTT
{
    public class UTTHub : Hub
    {
        public string UserName => Context.User.Identity.Name;

        public override Task OnConnectedAsync()
        {
            Clients.All.SendAsync("GameUpdated", Game.GetGames());
            return Clients.All.SendAsync("UserJoined", UserName);
        }

        public Task SendToLobby(string message)
        {
            return Clients.All.SendAsync("LobbyMessage", UserName, message);
        }

        public Task CreateGame()
        {
            var game = Game.CreateGame(UserName);
            return Clients.All.SendAsync("GameUpdated", Game.GetGames());
        }

        public Task JoinGame(int id)
        {
            var game = Game.JoinGame(id, UserName);
            return Clients.All.SendAsync("GameUpdated", Game.GetGames());
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return Clients.All.SendAsync("UserLeft", UserName);
        }
    }


    public class Game
    {
        private static ConcurrentDictionary<int, Game> _games = new ConcurrentDictionary<int, Game>();
        private static int _id;

        public int Id { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public string Status { get; set; }

        public static Game CreateGame(string player)
        {
            var id = Interlocked.Increment(ref _id);
            var game = new Game { Id = id, Player1 = player, Status = "Waiting" };
            _games.TryAdd(id, game);
            return game;
        }

        public static Game JoinGame(int id, string userName)
        {
            if (_games.TryGetValue(id, out var game) && game.Player2 == null)
            {
                game.Player2 = userName;
                game.Status = "Playing";
            }
            return game;
        }

        public static IEnumerable<Game> GetGames() => _games.Values;
    }
}