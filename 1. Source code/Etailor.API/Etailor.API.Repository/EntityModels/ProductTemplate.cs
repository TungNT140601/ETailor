using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ProductTemplate
    {
        public ProductTemplate()
        {
            Components = new HashSet<Component>();
            Products = new HashSet<Product>();
            TemplateBodySizes = new HashSet<TemplateBodySize>();
            TemplateStages = new HashSet<TemplateStage>();
        }

        public string Id { get; set; } = null!;
        public string CategoryId { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? ThumbnailImage { get; set; }
        public string? Image { get; set; }
        public string? CollectionImage { get; set; }
        public int? AveDateForComplete { get; set; }
        public string? UrlPath { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<Component> Components { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<TemplateBodySize> TemplateBodySizes { get; set; }
        public virtual ICollection<TemplateStage> TemplateStages { get; set; }
    }
}
