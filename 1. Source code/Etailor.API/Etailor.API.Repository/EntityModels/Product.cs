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
        public string? ProductTemplateId { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }
        public decimal? Price { get; set; }
        public int? Status { get; set; }
        public string? EvidenceImage { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Order? Order { get; set; }
        public virtual ProductTemplate? ProductTemplate { get; set; }
        public virtual ICollection<ProductBodySize> ProductBodySizes { get; set; }
        public virtual ICollection<ProductStage> ProductStages { get; set; }
    }
}
