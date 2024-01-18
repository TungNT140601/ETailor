using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class BodySize
    {
        public BodySize()
        {
            BodyAttributes = new HashSet<BodyAttribute>();
            CatalogBodySizes = new HashSet<CatalogBodySize>();
            ProductBodySizes = new HashSet<ProductBodySize>();
        }

        public string Id { get; set; } = null!;
        public string? BodyPart { get; set; }
        public int? BodyIndex { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public string? GuideVideoLink { get; set; }
        public decimal? MinValidValue { get; set; }
        public decimal? MaxValidValue { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<BodyAttribute> BodyAttributes { get; set; }
        public virtual ICollection<CatalogBodySize> CatalogBodySizes { get; set; }
        public virtual ICollection<ProductBodySize> ProductBodySizes { get; set; }
    }
}
