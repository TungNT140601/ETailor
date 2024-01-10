using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Customer
    {
        public Customer()
        {
            Chats = new HashSet<Chat>();
            Notifications = new HashSet<Notification>();
            Orders = new HashSet<Order>();
            ProfileBodyAttributes = new HashSet<ProfileBodyAttribute>();
        }

        public string Id { get; set; } = null!;
        public string? Avatar { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool? PhoneVerified { get; set; }
        public bool? EmailVerified { get; set; }
        public string? Otp { get; set; }
        public DateTime? OtpexpireTime { get; set; }
        public bool? Otpused { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual ICollection<Chat> Chats { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ProfileBodyAttribute> ProfileBodyAttributes { get; set; }
    }
}
