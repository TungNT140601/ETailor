using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ProductStage
    {
        public ProductStage()
        {
            ProductComponents = new HashSet<ProductComponent>();
            ProductStageMaterials = new HashSet<ProductStageMaterial>();
        }

        public string Id { get; set; } = null!;
        public string? StaffId { get; set; }
        public string? TemplateStageId { get; set; }
        public string? ProductId { get; set; }
        public int? StageNum { get; set; }
        public int? TaskIndex { get; set; }
        public string? EvidenceImage { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? Deadline { get; set; }
        public int? Status { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Product? Product { get; set; }
        public virtual Staff? Staff { get; set; }
        public virtual TemplateStage? TemplateStage { get; set; }
        public virtual ICollection<ProductComponent> ProductComponents { get; set; }
        public virtual ICollection<ProductStageMaterial> ProductStageMaterials { get; set; }
    }
}
