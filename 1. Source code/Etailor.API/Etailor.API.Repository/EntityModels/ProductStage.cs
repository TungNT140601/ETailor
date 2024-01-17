using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ProductStage
    {
        public ProductStage()
        {
            ProductComponents = new HashSet<ProductComponent>();
            SkillForStages = new HashSet<SkillForStage>();
        }

        public string Id { get; set; } = null!;
        public string? StaffId { get; set; }
        public string? CatalogStageId { get; set; }
        public string? ProductId { get; set; }
        public int? StageNum { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? Deadline { get; set; }

        public virtual CatalogStage? CatalogStage { get; set; }
        public virtual Product? Product { get; set; }
        public virtual Staff? Staff { get; set; }
        public virtual ICollection<ProductComponent> ProductComponents { get; set; }
        public virtual ICollection<SkillForStage> SkillForStages { get; set; }
    }
}
