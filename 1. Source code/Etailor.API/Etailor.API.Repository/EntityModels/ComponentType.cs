using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ComponentType
    {
        public ComponentType()
        {
            Components = new HashSet<Component>();
        }

        public string Id { get; set; } = null!;
        public string? CategoryId { get; set; }
        public string? CatalogStageId { get; set; }
        public string? Name { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual CatalogStage? CatalogStage { get; set; }
        public virtual Category? Category { get; set; }
        public virtual ICollection<Component> Components { get; set; }
    }
}
