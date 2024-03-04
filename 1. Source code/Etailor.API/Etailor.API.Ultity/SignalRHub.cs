using Etailor.API.Ultity.CommonValue;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Ultity
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

        public static Dictionary<string, string> StaffConnections
        {
            get
            {
                return _staffConnections;
            }
        }

        public static Dictionary<string, string> CustomerConnections
        {
            get
            {
                return _customerConnections;
            }
        }

        public override async Task<Task> OnConnectedAsync()
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
                if (role == RoleName.CUSTOMER)
                {
                    _customerConnections[userId] = Context.ConnectionId;
                    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                    await Groups.AddToGroupAsync(Context.ConnectionId, role);
                }
                else
                {
                    _staffConnections[userId] = Context.ConnectionId;
                    await Groups.AddToGroupAsync(Context.ConnectionId, "AllStaff");
                    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                    await Groups.AddToGroupAsync(Context.ConnectionId, role);
                }
            }
            return base.OnConnectedAsync();
        }

        public override async Task<Task> OnDisconnectedAsync(Exception exception)
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

            if (!string.IsNullOrEmpty(userId) && role == RoleName.CUSTOMER && _customerConnections.ContainsKey(userId))
            {
                _customerConnections.Remove(userId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            else if (!string.IsNullOrEmpty(userId) && role != RoleName.CUSTOMER && _staffConnections.ContainsKey(userId))
            {
                _staffConnections.Remove(userId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AllStaff");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, role);
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
