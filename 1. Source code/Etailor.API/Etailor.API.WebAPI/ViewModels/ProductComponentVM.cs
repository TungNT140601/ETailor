using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class ProductComponentOrderVM
    {
        public string? Id { get; set; }
        public string? ComponentId { get; set; }
        public string? ProductStageId { get; set; }
        public List<ProductComponentMaterialOrderVM>? ProductComponentMaterials { get; set; }
    }
}
