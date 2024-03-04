using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Ultity
{
    public class SignalRHub : Hub
    {
        private static Dictionary<string, string> _userConnections = new Dictionary<string, string>();

        public static string GetUserClientId(string userId)
        {
            _userConnections.TryGetValue(userId, out string connectionId);
            return connectionId;
        }

        public async Task SendMessageToUser(string userId, string message)
        {
            if (_userConnections.TryGetValue(userId, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
            }
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId) && _userConnections.ContainsKey(userId))
            {
                _userConnections.Remove(userId);
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
