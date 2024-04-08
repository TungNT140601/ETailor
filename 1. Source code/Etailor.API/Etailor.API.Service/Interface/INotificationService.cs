using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface INotificationService
    {
        Task<bool> AddNotification(string title, string message, string userId, string role);
        IEnumerable<Notification> GetNotifications(string userId, string role);
        Notification GetNotification(string notificationId, string userId, string role);
        Task<bool> ReadAllNotification(string userId, string role);
    }
}
