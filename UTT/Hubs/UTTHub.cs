using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace UTT
{
    public class UTTHub : Hub
    {
        public string UserName => Context.User?.Identity.Name;

        public override async Task OnConnectedAsync()
        {
            if (Context.User?.Identity.IsAuthenticated == true)
            {
                User.AddUser(UserName);
            }

            Interlocked.Increment(ref User.Count);

            await Clients.All.SendAsync("GameUpdated", Game.GetGames());
            await Clients.All.SendAsync("UsersChanged", User.Count);
        }

        [Authorize]
        public Task SendToLobby(string message)
        {
            return Clients.All.SendAsync("LobbyMessage", UserName, message);
        }

        [Authorize]
        public Task CreateGame(string name)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                return Task.CompletedTask;
            }

            Game.CreateGame(UserName, name);
            return Clients.All.SendAsync("GameUpdated", Game.GetGames());
        }

        [Authorize]
        public Task JoinGame(int id)
        {
            Game.JoinGame(id, UserName);
            return Clients.All.SendAsync("GameUpdated", Game.GetGames());
        }

        [Authorize]
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
            if (Context.User?.Identity.IsAuthenticated == true)
            {
                // TODO: Handle stopping games that include this user
                User.Remove(UserName);
            }

            Interlocked.Decrement(ref User.Count);

            return Clients.All.SendAsync("UsersChanged", User.Count);
        }
    }
}