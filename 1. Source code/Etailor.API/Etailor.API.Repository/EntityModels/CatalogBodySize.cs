using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class CatalogBodySize
    {
        public string Id { get; set; } = null!;
        public string? CatalogId { get; set; }
        public string? BodySizeId { get; set; }

        public virtual BodySize? BodySize { get; set; }
        public virtual Catalog? Catalog { get; set; }
    }
}
