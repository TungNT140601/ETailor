using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Customer
    {
        public Customer()
        {
            Notifications = new HashSet<Notification>();
            Orders = new HashSet<Order>();
            ProfileBodies = new HashSet<ProfileBody>();
        }

        public string Id { get; set; } = null!;
        public string? Avatar { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int? Gender { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Otpnumber { get; set; }
        public DateTime? OtptimeLimit { get; set; }
        public bool? Otpused { get; set; }
        public bool? PhoneVerified { get; set; }
        public bool? EmailVerified { get; set; }
        public string? SecrectKeyLogin { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ProfileBody> ProfileBodies { get; set; }
    }
}
