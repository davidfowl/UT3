using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

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

        public Task CreateGame(string name)
        {
            Game.CreateGame(UserName, name);
            return Clients.All.SendAsync("GameUpdated", Game.GetGames());
        }

        public Task JoinGame(int id)
        {
            Game.JoinGame(id, UserName);
            return Clients.All.SendAsync("GameUpdated", Game.GetGames());
        }

        public Task PlayTurn(int id, int outerRowIndex, int outerColIndex, int innerRowIndex, int innerColIndex)
        {
            if (Game.PlayTurn(UserName,
                              id,
                              outerRowIndex,
                              outerColIndex,
                              innerRowIndex,
                              innerColIndex,
                              out var nextMove))
            {
                return Clients.All.SendAsync("PlayMove", id, nextMove);
            }
            return Task.CompletedTask;
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
        public string Name { get; set; }

        private static ConcurrentDictionary<string, User> _users = new ConcurrentDictionary<string, User>();

        public static void AddUser(string user)
        {
            _users.TryAdd(user, new User
            {
                Name = user
            });
        }

        public static void Remove(string user)
        {
            _users.TryRemove(user, out _);
        }

        public static IEnumerable<User> GetUsers() => _users.Values;
    }
}