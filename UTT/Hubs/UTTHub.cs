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

        public override async Task OnConnectedAsync()
        {
            User.AddUser(UserName);

            await Clients.All.SendAsync("GameUpdated", Game.GetGames());
            await Clients.All.SendAsync("UsersChanged", User.GetUsers());
        }

        public Task SendToLobby(string message)
        {
            return Clients.All.SendAsync("LobbyMessage", UserName, message);
        }

        public Task CreateGame()
        {
            Game.CreateGame(UserName);
            return Clients.All.SendAsync("GameUpdated", Game.GetGames());
        }

        public Task JoinGame(int id)
        {
            Game.JoinGame(id, UserName);
            return Clients.All.SendAsync("GameUpdated", Game.GetGames());
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            // TODO: Handle stopping games that include this user

            User.Remove(UserName);

            return Clients.All.SendAsync("UsersChanged", User.GetUsers());
        }
    }

    public class User
    {
        private static ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();

        public static void AddUser(string user)
        {
            _users.TryAdd(user, user);
        }

        public static void Remove(string user)
        {
            _users.TryRemove(user, out _);
        }

        public static IEnumerable<string> GetUsers() => _users.Values;
    }

    public class Game
    {
        private static ConcurrentDictionary<int, Game> _games = new ConcurrentDictionary<int, Game>();
        private static int _id;

        public int Id { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public GameStatus Status { get; set; }

        public static void CreateGame(string player)
        {
            var id = Interlocked.Increment(ref _id);
            var game = new Game
            {
                Id = id,
                Player1 = player,
                Status = GameStatus.Waiting
            };
            _games.TryAdd(id, game);
        }

        public static void JoinGame(int id, string userName)
        {
            if (_games.TryGetValue(id, out var game) && game.Player2 == null)
            {
                game.Player2 = userName;
                game.Status = GameStatus.Playing;
            }
        }

        public static IEnumerable<Game> GetGames() => _games.Values;
    }

    public enum GameStatus
    {
        Waiting,
        Playing,
        Completed
    }
}