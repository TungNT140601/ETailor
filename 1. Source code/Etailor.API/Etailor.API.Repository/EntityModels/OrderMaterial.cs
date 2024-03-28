using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class OrderMaterial
    {
        public string Id { get; set; } = null!;
        public string? MaterialId { get; set; }
        public string? OrderId { get; set; }
        public string? Image { get; set; }
        public decimal? Value { get; set; }
        public decimal? ValueUsed { get; set; }
        public bool? IsCusMaterial { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Material? Material { get; set; }
        public virtual Order? Order { get; set; }
    }
}
