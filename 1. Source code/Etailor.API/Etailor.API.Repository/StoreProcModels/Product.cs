using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.StoreProcModels
{
    public partial class Product
    {

        public string ProductId { get; set; } = null!;
        public string? OrderId { get; set; }
        public string? ProductTemplateId { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }
        public decimal? Price { get; set; }
        public int? ProductStatus { get; set; }
        public string? FabricMaterialId { get; set; }

        public virtual Material? FabricMaterial { get; set; }
        public virtual Order? Order { get; set; }
    }
}
