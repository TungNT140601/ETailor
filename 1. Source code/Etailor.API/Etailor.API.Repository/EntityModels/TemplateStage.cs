using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class TemplateStage
    {
        public TemplateStage()
        {
            ComponentStages = new HashSet<ComponentStage>();
            InverseTemplateStageNavigation = new HashSet<TemplateStage>();
            ProductStages = new HashSet<ProductStage>();
        }

        public string Id { get; set; } = null!;
        public string? ProductTemplateId { get; set; }
        public string? TemplateStageId { get; set; }
        public string? Name { get; set; }
        public int? StageNum { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual ProductTemplate? ProductTemplate { get; set; }
        public virtual TemplateStage? TemplateStageNavigation { get; set; }
        public virtual ICollection<ComponentStage> ComponentStages { get; set; }
        public virtual ICollection<TemplateStage> InverseTemplateStageNavigation { get; set; }
        public virtual ICollection<ProductStage> ProductStages { get; set; }
    }
}
