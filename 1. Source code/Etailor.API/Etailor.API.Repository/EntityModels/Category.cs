using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Category
    {
        public Category()
        {
            Catalogs = new HashSet<Catalog>();
            ComponentTypes = new HashSet<ComponentType>();
        }

        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<Catalog> Catalogs { get; set; }
        public virtual ICollection<ComponentType> ComponentTypes { get; set; }
    }
}
