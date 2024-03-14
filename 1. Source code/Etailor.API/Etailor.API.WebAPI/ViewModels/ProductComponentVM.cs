using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class ProductComponentOrderVM
    {
        public string? Id { get; set; }
        public string? ComponentId { get; set; }
    }
    public class ProductComponentTaskDetailVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }

        public virtual ComponentOrderVM? Component { get; set; }
        public virtual ICollection<ProductComponentMaterialOrderVM>? ProductComponentMaterials { get; set; }
    }
}
