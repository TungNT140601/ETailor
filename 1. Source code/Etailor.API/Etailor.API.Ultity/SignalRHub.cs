using Etailor.API.Ultity.CommonValue;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Serilog;
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
        private static List<Dictionary<string, Dictionary<string, string>>> _groupStaffConnections = new List<Dictionary<string, Dictionary<string, string>>>();
        private static List<Dictionary<string, Dictionary<string, string>>> _groupCustomerConnections = new List<Dictionary<string, Dictionary<string, string>>>();

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

        public static List<Dictionary<string, Dictionary<string, string>>> GroupStaffConnections
        {
            get
            {
                return _groupStaffConnections;
            }
        }

        public static List<Dictionary<string, Dictionary<string, string>>> GroupCustomerConnections
        {
            get
            {
                return _groupCustomerConnections;
            }
        }

        public override async Task<Task> OnConnectedAsync()
        {
            try
            {
                Log.Information($"New Connection: {Context.ConnectionId}");

                var accessToken = Context.GetHttpContext().Request.Query["access_token"];
                var requestInfo = new
                {
                    Path = Context.GetHttpContext().Request.Path,
                    Method = Context.GetHttpContext().Request.Method,
                    QueryString = Context.GetHttpContext().Request.QueryString,
                    Query = Context.GetHttpContext().Request.Query,
                    Headers = Context.GetHttpContext().Request.Headers,
                    // Add more properties as needed
                };

                Log.Information($"Connection {Context.ConnectionId} requestInfo: " + JsonConvert.SerializeObject(requestInfo).ToString());

                Log.Information($"Connection {Context.ConnectionId} Headers Authorization: " + Context.GetHttpContext().Request.Headers["Authorization"].ToString());

                Log.Information($"Connection {Context.ConnectionId} type access_token: " + accessToken.GetType());

                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = Context.GetHttpContext().Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                }

                Log.Information($"Connection {Context.ConnectionId} access_token: " + accessToken);
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
                var name = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                Log.Information($"Connection: {Context.ConnectionId} " + (!string.IsNullOrEmpty(userId) ? "token accept" : "token not accept"));
                if (!string.IsNullOrEmpty(userId))
                {
                    if (role == RoleName.CUSTOMER)
                    {
                        _customerConnections[userId] = Context.ConnectionId;

                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, role);

                        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                        await Groups.AddToGroupAsync(Context.ConnectionId, role);
                    }
                    else
                    {
                        _staffConnections[userId] = Context.ConnectionId;

                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AllStaff");
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, role);

                        await Groups.AddToGroupAsync(Context.ConnectionId, "AllStaff");
                        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                        await Groups.AddToGroupAsync(Context.ConnectionId, role);
                    }
                }
                Log.Information($"Add User {userId} Role {role} Name {name} Connection: {Context.ConnectionId}");

                return base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Connection ERROR: {Context.ConnectionId} :" + ex.Message);
                throw;
            }
        }

        public override async Task<Task> OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var accessToken = Context.GetHttpContext().Request.Query["access_token"];

                if (accessToken == String.Empty)
                {
                    accessToken = Context.GetHttpContext().Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                }
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
                var name = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

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
                Log.Information($"Remove User {userId} Role {role} Name {name} Connection: {Context.ConnectionId}");

                return base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                Log.Error($"Connection ERROR: {Context.ConnectionId} :" + ex.Message);
                throw;
            }
        }
    }
}
