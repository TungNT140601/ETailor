using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class MaterialForComponent
    {
        public string Id { get; set; } = null!;
        public string? MaterialId { get; set; }
        public string? OrderMaterialId { get; set; }
        public string? ProductComponentId { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public int? Quantity { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Material? Material { get; set; }
        public virtual OrderMaterial? OrderMaterial { get; set; }
        public virtual ProductComponent? ProductComponent { get; set; }
    }
}
