using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
            ProductBodySizes = new HashSet<ProductBodySize>();
            ProductComponents = new HashSet<ProductComponent>();
            ProductStages = new HashSet<ProductStage>();
        }

        public string Id { get; set; } = null!;
        public string? ProductCategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Image { get; set; }
        public string? UrlPath { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }
        public bool? IsCustomize { get; set; }

        public virtual ProductCategory? ProductCategory { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<ProductBodySize> ProductBodySizes { get; set; }
        public virtual ICollection<ProductComponent> ProductComponents { get; set; }
        public virtual ICollection<ProductStage> ProductStages { get; set; }
    }
}
