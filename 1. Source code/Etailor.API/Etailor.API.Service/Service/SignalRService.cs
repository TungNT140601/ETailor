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

        public async Task SendNotificationToUser(string userId, string Notification, string role)
        {
            if (role == RoleName.CUSTOMER)
            {
                //_customerConnections.TryGetValue(userId, out string customerConnectionId);
                //await hubContext.Clients.Client(customerConnectionId).SendAsync("CustomerReceiveNotification", Notification);
                await hubContext.Clients.Group(userId).SendAsync("CustomerReceiveNotification", Notification);
            }
            else
            {
                SignalRHub.StaffConnections.TryGetValue(userId, out string staffConnectionId);
                //await hubContext.Clients.Client(staffConnectionId).SendAsync("ReceiveNotification", Notification);
                await hubContext.Clients.Group(userId).SendAsync("StaffReceiveNotification", Notification);
            }
        }

        public async Task SendNotificationToAllStaff(string Notification)
        {
            await hubContext.Clients.Group("AllStaff").SendAsync("AllStaffReceiveNotification", Notification);
        }

        public async Task SendNotificationToManager(string Notification)
        {
            await hubContext.Clients.Group(RoleName.MANAGER).SendAsync("ManagersfReceiveNotification", Notification);
        }

        public async Task SendNotificationToStaff(string Notification)
        {
            await hubContext.Clients.Group(RoleName.STAFF).SendAsync("StaffsReceiveNotification", Notification);
        }

        public async Task SendNotificationToAdmin(string Notification)
        {
            await hubContext.Clients.Group(RoleName.ADMIN).SendAsync("AdminsReceiveNotification", Notification);
        }

        public async Task SendNotificationToCustomer(string Notification)
        {
            await hubContext.Clients.Group(RoleName.CUSTOMER).SendAsync("CustomersReceiveNotification", Notification);
        }

        public async Task SendVNPayResult(string Notification)
        {
            await hubContext.Clients.Group("AllStaff").SendAsync("VNPayResult", Notification);
        }

        public async Task CheckMessage(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                await hubContext.Clients.Group("AllStaff").SendAsync("ChatWithUs", "Have message");
            }
            else
            {
                await hubContext.Clients.Group(id).SendAsync("ChatWithUs", "Have message");
            }
        }
    }
}
