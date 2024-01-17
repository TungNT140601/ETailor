using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Staff
    {
        public Staff()
        {
            Blogs = new HashSet<Blog>();
            ChatHistories = new HashSet<ChatHistory>();
            OrderMaterials = new HashSet<OrderMaterial>();
            Orders = new HashSet<Order>();
            ProductStages = new HashSet<ProductStage>();
            ProfileBodies = new HashSet<ProfileBody>();
            SkillOfStaffs = new HashSet<SkillOfStaff>();
        }

        public string Id { get; set; } = null!;
        public int? Role { get; set; }
        public string? Avatar { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<Blog> Blogs { get; set; }
        public virtual ICollection<ChatHistory> ChatHistories { get; set; }
        public virtual ICollection<OrderMaterial> OrderMaterials { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ProductStage> ProductStages { get; set; }
        public virtual ICollection<ProfileBody> ProfileBodies { get; set; }
        public virtual ICollection<SkillOfStaff> SkillOfStaffs { get; set; }
    }
}
