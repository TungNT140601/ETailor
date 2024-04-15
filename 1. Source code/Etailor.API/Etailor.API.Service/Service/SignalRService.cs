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

        public async Task SendNotificationToUser(string userId, string Notification)
        {
            await hubContext.Clients.Group(userId).SendAsync("Notification", Notification);
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

        public async Task SendNotificationToManagers(string Notification)
        {
            await hubContext.Clients.Group(RoleName.MANAGER).SendAsync("Notification", Notification);
        }
    }
}
