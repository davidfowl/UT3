using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace UTT
{
    public class UTTHub : Hub
    {
        public string UserName => Context.User.Identity.Name;

        public override Task OnConnectedAsync()
        {
            return Clients.All.SendAsync("UserJoined", UserName);
        }

        public Task SendToLobby(string message)
        {
            return Clients.All.SendAsync("LobbyMessage", UserName, message);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return Clients.All.SendAsync("UserLeft", UserName);
        }
    }
}