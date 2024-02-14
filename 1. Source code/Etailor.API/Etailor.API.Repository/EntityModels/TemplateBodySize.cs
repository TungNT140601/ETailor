using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class TemplateBodySize
    {
        public string Id { get; set; } = null!;
        public string? ProductTemplateId { get; set; }
        public string? BodySizeId { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }


        public virtual BodySize? BodySize { get; set; }
        public virtual ProductTemplate? ProductTemplate { get; set; }
    }
}
