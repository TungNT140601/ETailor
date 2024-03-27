using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class ProductComponentMaterialOrderVM
    {
        public string? Id { get; set; }
        public string? MaterialId { get; set; }
        public decimal? Quantity { get; set; }

        public virtual Material? Material { get; set; }
    }
}
