using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class BodyAttribute
    {
        public string Id { get; set; } = null!;
        public string? ProfileBodyAttributeId { get; set; }
        public string? BodySizeId { get; set; }
        public decimal? Value { get; set; }
        public string? Measure { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual BodySize? BodySize { get; set; }
        public virtual ProfileBodyAttribute? ProfileBodyAttribute { get; set; }
    }
}
