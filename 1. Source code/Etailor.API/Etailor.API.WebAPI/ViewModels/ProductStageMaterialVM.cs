using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class ProductStageMaterialOrderVM
    {
        public string? Id { get; set; }
        public string? MaterialId { get; set; }
        public decimal? Quantity { get; set; }

        public virtual MaterialVM? Material { get; set; }
    }
}
