using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ProductStageMaterial
    {
        public string Id { get; set; } = null!;
        public string? ProductStageId { get; set; }
        public string? MaterialId { get; set; }
        public decimal? Quantity { get; set; }

        public virtual Material? Material { get; set; }
        public virtual ProductStage? ProductStage { get; set; }
    }
}
