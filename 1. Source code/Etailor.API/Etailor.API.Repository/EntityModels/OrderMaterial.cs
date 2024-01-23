using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class OrderMaterial
    {
        public string Id { get; set; } = null!;
        public string? MaterialId { get; set; }
        public string? OrderId { get; set; }
        public string? Approver { get; set; }
        public string? Image { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? ApproveTime { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Staff? ApproverNavigation { get; set; }
        public virtual Material? Material { get; set; }
        public virtual Order? Order { get; set; }
    }
}
