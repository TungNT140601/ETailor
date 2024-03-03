using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ProfileBody
    {
        public ProfileBody()
        {
            BodyAttributes = new HashSet<BodyAttribute>();
            Products = new HashSet<Product>();
        }

        public string Id { get; set; } = null!;
        public string? CustomerId { get; set; }
        public string? StaffId { get; set; }
        public string? Name { get; set; }
        public bool? IsLocked { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual Staff? Staff { get; set; }
        public virtual ICollection<BodyAttribute> BodyAttributes { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
