using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class ProductComponent
    {
        public ProductComponent()
        {
            ProductComponentMaterials = new HashSet<ProductComponentMaterial>();
        }

        public string Id { get; set; } = null!;
        public string? ComponentId { get; set; }
        public string? ProductStageId { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public string? Note { get; set; }
        public string? NoteImage { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }

        public virtual Component? Component { get; set; }
        public virtual ProductStage? ProductStage { get; set; }
        public virtual ICollection<ProductComponentMaterial> ProductComponentMaterials { get; set; }
    }
}
