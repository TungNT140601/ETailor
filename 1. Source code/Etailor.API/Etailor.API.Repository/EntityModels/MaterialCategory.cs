using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class MaterialCategory
    {
        public MaterialCategory()
        {
            Materials = new HashSet<Material>();
        }

        public string Id { get; set; } = null!;
        public string? MaterialTypeId { get; set; }
        public string? Name { get; set; }
        public double? PricePerUnit { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual MaterialType? MaterialType { get; set; }
        public virtual ICollection<Material> Materials { get; set; }
    }
}
