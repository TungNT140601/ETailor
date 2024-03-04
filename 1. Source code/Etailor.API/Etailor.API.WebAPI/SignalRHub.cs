using Etailor.API.Ultity.CommonValue;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.SignalR;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Etailor.API.WebAPI
{
    public class SignalRHub : Hub
    {
        private static Dictionary<string, string> _staffConnections = new Dictionary<string, string>();
        private static Dictionary<string, string> _customerConnections = new Dictionary<string, string>();

        public static string GetUserClientId(string userId, string role)
        {
            if (role == RoleName.CUSTOMER)
            {
                _customerConnections.TryGetValue(userId, out string customerConnectionId);
                return customerConnectionId;
            }
            else
            {
                _staffConnections.TryGetValue(userId, out string staffConnectionId);
                return staffConnectionId;
            }
        }

        public async Task SendMessageToUser(string userId, string message, string role)
        {
            if (role == RoleName.CUSTOMER)
            {
                _customerConnections.TryGetValue(userId, out string customerConnectionId);
                await Clients.Client(customerConnectionId).SendAsync("ReceiveMessage", message);
            }
            else
            {
                _staffConnections.TryGetValue(userId, out string staffConnectionId);
                await Clients.Client(staffConnectionId).SendAsync("ReceiveMessage", message);
            }
        }

        public async Task AddUser(string userId, string role)
        {
            if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(role))
            {
                if (role == RoleName.CUSTOMER)
                {
                    _customerConnections[userId] = Context.ConnectionId;
                }
                else
                {
                    _staffConnections[userId] = Context.ConnectionId;
                }
            }
        }

        public override Task OnConnectedAsync()
        {
            var accessToken = Context.GetHttpContext().Request.Query["access_token"];
            // Decode the JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(accessToken) as JwtSecurityToken;

            // Create claims from the JWT token
            var claims = jwtToken?.Claims;

            // Create a ClaimsIdentity from the claims
            var identity = new ClaimsIdentity(claims, "Bearer");

            // Create a ClaimsPrincipal from the ClaimsIdentity
            var user = new ClaimsPrincipal(identity);

            var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var role = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                if(role == RoleName.CUSTOMER)
                {
                    _customerConnections[userId] = Context.ConnectionId;
                }
                else
                {
                    _staffConnections[userId] = Context.ConnectionId;
                }
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId) && _staffConnections.ContainsKey(userId))
            {
                _staffConnections.Remove(userId);
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
