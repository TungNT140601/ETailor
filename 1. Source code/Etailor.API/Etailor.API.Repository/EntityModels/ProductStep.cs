using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ProductStep
    {
        public ProductStep()
        {
            ProductComponents = new HashSet<ProductComponent>();
        }

        public string Id { get; set; } = null!;
        public string? ProductStageId { get; set; }
        public string? Name { get; set; }
        public bool? IsFinish { get; set; }
        public DateTime? FinishedTime { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual ProductStage? ProductStage { get; set; }
        public virtual ICollection<ProductComponent> ProductComponents { get; set; }
    }
}
