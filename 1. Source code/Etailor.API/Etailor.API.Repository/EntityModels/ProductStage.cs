using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ProductStage
    {
        public ProductStage()
        {
            ProductSteps = new HashSet<ProductStep>();
        }

        public string Id { get; set; } = null!;
        public string? ProductId { get; set; }
        public string? Name { get; set; }
        public string? MakerId { get; set; }
        public int? StageNum { get; set; }
        public decimal? StageProcess { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Staff? Maker { get; set; }
        public virtual Product? Product { get; set; }
        public virtual ICollection<ProductStep> ProductSteps { get; set; }
    }
}
