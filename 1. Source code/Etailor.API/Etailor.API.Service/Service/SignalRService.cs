using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<SignalRHub> hubContext;
        public SignalRService(IHubContext<SignalRHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public async Task SendMessageToUser(string userId, string message, string role)
        {
            if (role == RoleName.CUSTOMER)
            {
                //_customerConnections.TryGetValue(userId, out string customerConnectionId);
                //await hubContext.Clients.Client(customerConnectionId).SendAsync("CustomerReceiveMessage", message);
                await hubContext.Clients.Group(userId).SendAsync("CustomerReceiveMessage", message);
            }
            else
            {
                SignalRHub.StaffConnections.TryGetValue(userId, out string staffConnectionId);
                //await hubContext.Clients.Client(staffConnectionId).SendAsync("ReceiveMessage", message);
                await hubContext.Clients.Group(userId).SendAsync("StaffReceiveMessage", message);
            }
        }

        public async Task SendMessageToAllStaff(string message)
        {
            await hubContext.Clients.Group("AllStaff").SendAsync("AllStaffReceiveMessage", message);
        }

        public async Task SendMessageToManager(string message)
        {
            await hubContext.Clients.Group(RoleName.MANAGER).SendAsync("ManagersfReceiveMessage", message);
        }

        public async Task SendMessageToStaff(string message)
        {
            await hubContext.Clients.Group(RoleName.STAFF).SendAsync("StaffsReceiveMessage", message);
        }

        public async Task SendMessageToAdmin(string message)
        {
            await hubContext.Clients.Group(RoleName.ADMIN).SendAsync("AdminsReceiveMessage", message);
        }

        public async Task SendMessageToCustomer(string message)
        {
            await hubContext.Clients.Group(RoleName.CUSTOMER).SendAsync("CustomersReceiveMessage", message);
        }

        public async Task SendVNPayResult(string message)
        {
            await hubContext.Clients.Group("AllStaff").SendAsync("VNPayResult", message);
        }
    }
}
