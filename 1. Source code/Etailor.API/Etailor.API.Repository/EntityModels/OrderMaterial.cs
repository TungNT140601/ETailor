using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class OrderMaterial
    {
        public OrderMaterial()
        {
            MaterialForComponents = new HashSet<MaterialForComponent>();
        }

        public string Id { get; set; } = null!;
        public string? OrderId { get; set; }
        public string? MaterialCategoryId { get; set; }
        public string? Name { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public string? Image { get; set; }
        public bool? InProcess { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? ApproveTime { get; set; }
        public string? Approver { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Staff? ApproverNavigation { get; set; }
        public virtual MaterialCategory? MaterialCategory { get; set; }
        public virtual Order? Order { get; set; }
        public virtual ICollection<MaterialForComponent> MaterialForComponents { get; set; }
    }
}
