using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Etailor.API.Repository.EntityModels
{
    public partial class TemplateBodySize
    {
        public string Id { get; set; } = null!;
        public string? ProductTemplateId { get; set; }
        public string? BodySizeId { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        [JsonIgnore]
        public virtual BodySize? BodySize { get; set; }
        [JsonIgnore]
        public virtual ProductTemplate? ProductTemplate { get; set; }
    }
}
