using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class MaterialCategory
    {
        public MaterialCategory()
        {
            Materials = new HashSet<Material>();
            OrderMaterials = new HashSet<OrderMaterial>();
        }

        public string Id { get; set; } = null!;
        public string? MaterialTypeId { get; set; }
        public string? Name { get; set; }
        public bool? Handled { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual MaterialType? MaterialType { get; set; }
        public virtual ICollection<Material> Materials { get; set; }
        public virtual ICollection<OrderMaterial> OrderMaterials { get; set; }
    }
}
