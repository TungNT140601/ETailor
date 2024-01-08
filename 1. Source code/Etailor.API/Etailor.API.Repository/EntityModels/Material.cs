using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Material
    {
        public Material()
        {
            MaterialForComponents = new HashSet<MaterialForComponent>();
        }

        public string Id { get; set; } = null!;
        public string? MaterialCategoryId { get; set; }
        public string? Name { get; set; }
        public string? Measure { get; set; }
        public string? Image { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual MaterialCategory? MaterialCategory { get; set; }
        public virtual ICollection<MaterialForComponent> MaterialForComponents { get; set; }
    }
}
