using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Material
    {
        public Material()
        {
            OrderMaterials = new HashSet<OrderMaterial>();
            ProductComponentMaterials = new HashSet<ProductComponentMaterial>();
        }

        public string Id { get; set; } = null!;
        public string? MaterialCategoryId { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public decimal? Quantity { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual MaterialCategory? MaterialCategory { get; set; }
        public virtual ICollection<OrderMaterial> OrderMaterials { get; set; }
        public virtual ICollection<ProductComponentMaterial> ProductComponentMaterials { get; set; }
    }
}
