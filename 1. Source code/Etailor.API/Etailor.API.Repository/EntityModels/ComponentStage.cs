using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ComponentStage
    {
        public string Id { get; set; } = null!;
        public string? ComponentTypeId { get; set; }
        public string? CatalogStageId { get; set; }

        public virtual CatalogStage? CatalogStage { get; set; }
        public virtual ComponentType? ComponentType { get; set; }
    }
}
