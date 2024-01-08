using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ComponentStyle
    {
        public string Id { get; set; } = null!;
        public string? ProductComponentId { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual ProductComponent? ProductComponent { get; set; }
    }
}
