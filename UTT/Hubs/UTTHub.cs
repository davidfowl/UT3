using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace UTT
{
    public class UTTHub : Hub
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UTTHub(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public string UserName => _userManager.GetUserName(Context.User);

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