using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class BodyAttribute
    {
        public string Id { get; set; } = null!;
        public string? ProfileBodyId { get; set; }
        public string? BodySizeId { get; set; }
        public decimal? Value { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual BodySize? BodySize { get; set; }
        public virtual ProfileBody? ProfileBody { get; set; }
    }
}
