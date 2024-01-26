using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ProductComponentMaterial
    {
        public string Id { get; set; } = null!;
        public string? ProductComponentId { get; set; }
        public string? MaterialId { get; set; }
        public decimal? Quantity { get; set; }

        public virtual Material? Material { get; set; }
        public virtual ProductComponent? ProductComponent { get; set; }
    }
}
