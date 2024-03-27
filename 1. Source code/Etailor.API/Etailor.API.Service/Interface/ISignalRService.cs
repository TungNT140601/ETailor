using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface ISignalRService
    {
        Task SendNotificationToUser(string userId, string Notification);
        Task SendVNPayResult(string Notification);
        Task CheckMessage(string id);
    }
}
