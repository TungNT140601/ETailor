using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ProfileBodyAttribute
    {
        public ProfileBodyAttribute()
        {
            BodyAttributes = new HashSet<BodyAttribute>();
            OrderDetails = new HashSet<OrderDetail>();
        }

        public string Id { get; set; } = null!;
        public string? CustomerId { get; set; }
        public string? Name { get; set; }
        public bool? IsLocked { get; set; }
        public bool? IsByStaff { get; set; }
        public string? MakerId { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual Staff? Maker { get; set; }
        public virtual ICollection<BodyAttribute> BodyAttributes { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
