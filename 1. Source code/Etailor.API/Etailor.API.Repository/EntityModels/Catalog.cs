using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Catalog
    {
        public Catalog()
        {
            CatalogBodySizes = new HashSet<CatalogBodySize>();
            CatalogStages = new HashSet<CatalogStage>();
            Components = new HashSet<Component>();
            Products = new HashSet<Product>();
        }

        public string Id { get; set; } = null!;
        public string? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Image { get; set; }
        public string? UrlPath { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Category? Category { get; set; }
        public virtual ICollection<CatalogBodySize> CatalogBodySizes { get; set; }
        public virtual ICollection<CatalogStage> CatalogStages { get; set; }
        public virtual ICollection<Component> Components { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
