using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ProductComponent
    {
        public ProductComponent()
        {
            ComponentStyles = new HashSet<ComponentStyle>();
            MaterialForComponents = new HashSet<MaterialForComponent>();
        }

        public string Id { get; set; } = null!;
        public string? ProductId { get; set; }
        public string? ProductStepId { get; set; }
        public string? Name { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Product? Product { get; set; }
        public virtual ProductStep? ProductStep { get; set; }
        public virtual ICollection<ComponentStyle> ComponentStyles { get; set; }
        public virtual ICollection<MaterialForComponent> MaterialForComponents { get; set; }
    }
}
