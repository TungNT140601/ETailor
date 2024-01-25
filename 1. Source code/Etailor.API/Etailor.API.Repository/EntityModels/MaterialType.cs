using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class MaterialType
    {
        public MaterialType()
        {
            MaterialCategories = new HashSet<MaterialCategory>();
        }

        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Unit { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<MaterialCategory> MaterialCategories { get; set; }
    }
}
