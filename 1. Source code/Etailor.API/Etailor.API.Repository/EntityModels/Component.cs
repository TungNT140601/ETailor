using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Component
    {
        public Component()
        {
            ProductComponents = new HashSet<ProductComponent>();
        }

        public string Id { get; set; } = null!;
        public string? ComponentTypeId { get; set; }
        public string? ProductTemplateId { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }
        public int? Index { get; set; }
        public bool? Default { get; set; }

        public virtual ComponentType? ComponentType { get; set; }
        public virtual ProductTemplate? ProductTemplate { get; set; }
        public virtual ICollection<ProductComponent> ProductComponents { get; set; }
    }
}
