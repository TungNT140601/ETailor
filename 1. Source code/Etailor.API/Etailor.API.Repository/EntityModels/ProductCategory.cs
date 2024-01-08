using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ProductCategory
    {
        public ProductCategory()
        {
            Products = new HashSet<Product>();
        }

        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
