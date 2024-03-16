using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface ISignalRService
    {
        Task SendNotificationToUser(string userId, string Notification, string role);
        Task SendNotificationToAllStaff(string Notification);
        Task SendNotificationToManager(string Notification);
        Task SendNotificationToStaff(string Notification);
        Task SendNotificationToAdmin(string Notification);
        Task SendNotificationToCustomer(string Notification);
        Task SendVNPayResult(string Notification);
        Task CheckMessage(string id);
    }
}
