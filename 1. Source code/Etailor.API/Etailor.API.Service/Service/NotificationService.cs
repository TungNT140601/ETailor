using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface;
using Etailor.API.Repository.StoreProcModels;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Etailor.API.Ultity.CustomException;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository notificationRepository;
        private readonly ICustomerRepository customerRepository;
        private readonly IStaffRepository staffRepository;
        private readonly ISignalRService signalRService;

        public NotificationService(INotificationRepository notificationRepository, ICustomerRepository customerRepository, IStaffRepository staffRepository, ISignalRService signalRService)
        {
            this.notificationRepository = notificationRepository;
            this.customerRepository = customerRepository;
            this.staffRepository = staffRepository;
            this.signalRService = signalRService;
        }

        public async Task<bool> AddNotification(string title, string message, string userId, string role)
        {
            var notification = new Notification
            {
                Id = Ultils.GenGuidString(),
                IsActive = true,
                IsRead = false,
                Content = message,
                SendTime = DateTime.UtcNow.AddHours(7),
                ReadTime = null,
                Title = title,
                CustomerId = role == RoleName.CUSTOMER ? userId : null,
                StaffId = role == RoleName.CUSTOMER ? null : userId
            };

            if (notificationRepository.Create(notification))
            {
                await signalRService.SendNotificationToUser(userId, JsonConvert.SerializeObject(notification));

                return true;
            }

            return false;
        }

        public IEnumerable<Notification> GetNotifications(string userId, string role)
        {
            if (role == RoleName.CUSTOMER)
            {
                var notifications = notificationRepository.GetAll(X => X.CustomerId == userId && X.IsActive == true);
                if (notifications != null && notifications.Any())
                {
                    notifications = notifications.OrderByDescending(X => X.SendTime);
                }
                return notifications;
            }
            else
            {
                var notifications = notificationRepository.GetAll(X => X.StaffId == userId && X.IsActive == true);
                if (notifications != null && notifications.Any())
                {
                    notifications = notifications.OrderByDescending(X => X.SendTime);
                }
                return notifications;
            }
        }

        public Notification GetNotification(string notificationId, string userId, string role)
        {
            var notification = notificationRepository.Get(notificationId);
            if (role == RoleName.CUSTOMER)
            {
                if (notification.CustomerId == userId && notification.IsActive == true)
                {
                    if (notification.IsRead == false)
                    {
                        notification.IsRead = true;
                        notification.ReadTime = DateTime.UtcNow.AddHours(7);
                        notificationRepository.Update(notificationId, notification);
                    }
                    return notification;
                }
            }
            else
            {
                if (notification.StaffId == userId && notification.IsActive == true)
                {
                    if (notification.IsRead == false)
                    {
                        notification.IsRead = true;
                        notification.ReadTime = DateTime.UtcNow.AddHours(7);
                        notificationRepository.Update(notificationId, notification);
                    }
                    return notification;
                }
            }
            return null;
        }

        public async Task<bool> ReadAllNotification(string userId, string role)
        {
            var userIdParam = new SqlParameter()
            {
                DbType = System.Data.DbType.String,
                ParameterName = "@UserId",
                Value = userId
            };
            var roleParam = new SqlParameter()
            {
                DbType = System.Data.DbType.Int32,
                ParameterName = "@Role",
                Value = role == RoleName.CUSTOMER ? 3 : 1
            };

            try
            {
                var result = await notificationRepository.GetStoreProcedureReturnInt(StoreProcName.Read_All_Notification, userIdParam, roleParam);
                if (result == 1)
                {
                    return true;
                }
                else
                {
                    throw new SystemsException("Lỗi khi cập nhật thông báo", nameof(NotificationService.ReadAllNotification));
                }
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }
    }
}
