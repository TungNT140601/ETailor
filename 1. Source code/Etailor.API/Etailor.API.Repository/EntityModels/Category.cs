using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Category
    {
        public Category()
        {
            ComponentTypes = new HashSet<ComponentType>();
            ProductTemplates = new HashSet<ProductTemplate>();
        }

        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<ComponentType> ComponentTypes { get; set; }
        public virtual ICollection<ProductTemplate> ProductTemplates { get; set; }
    }
}
