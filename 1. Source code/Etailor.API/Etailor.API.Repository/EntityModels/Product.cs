using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Product
    {
        public Product()
        {
            ProductBodySizes = new HashSet<ProductBodySize>();
            ProductStages = new HashSet<ProductStage>();
        }

        public string Id { get; set; } = null!;
        public string? OrderId { get; set; }
        public string? CatalogId { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }
        public int? Status { get; set; }
        public decimal? StatusPercent { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Catalog? Catalog { get; set; }
        public virtual Order? Order { get; set; }
        public virtual ICollection<ProductBodySize> ProductBodySizes { get; set; }
        public virtual ICollection<ProductStage> ProductStages { get; set; }
    }
}
