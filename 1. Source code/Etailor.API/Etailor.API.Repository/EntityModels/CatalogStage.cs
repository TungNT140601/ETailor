using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class CatalogStage
    {
        public CatalogStage()
        {
            ComponentTypes = new HashSet<ComponentType>();
            InverseCatalogStageNavigation = new HashSet<CatalogStage>();
            ProductStages = new HashSet<ProductStage>();
        }

        public string Id { get; set; } = null!;
        public string? CatalogId { get; set; }
        public string? CatalogStageId { get; set; }
        public string? Name { get; set; }
        public int? StageNum { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Catalog? Catalog { get; set; }
        public virtual CatalogStage? CatalogStageNavigation { get; set; }
        public virtual ICollection<ComponentType> ComponentTypes { get; set; }
        public virtual ICollection<CatalogStage> InverseCatalogStageNavigation { get; set; }
        public virtual ICollection<ProductStage> ProductStages { get; set; }
    }
}
