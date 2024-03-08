using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface ISignalRService
    {
        Task SendMessageToUser(string userId, string message, string role);
        Task SendMessageToAllStaff(string message);
        Task SendMessageToManager(string message);
        Task SendMessageToStaff(string message);
        Task SendMessageToAdmin(string message);
        Task SendMessageToCustomer(string message);
        Task SendVNPayResult(string message);
    }
}
