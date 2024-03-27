using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.StoreProcModels
{
    public partial class Material
    {
        public Material()
        {
            Products = new HashSet<Product>();
        }

        public string MaterialId { get; set; } = null!;
        public string? MaterialCategoryId { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public decimal? Quantity { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
