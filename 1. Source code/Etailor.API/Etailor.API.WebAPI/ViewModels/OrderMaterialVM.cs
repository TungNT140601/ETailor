using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class OrderMaterialVM
    {
        public string? Id { get; set; }
        public string? MaterialId { get; set; }
        public string? OrderId { get; set; }
        public string? Image { get; set; }
        public decimal? Value { get; set; }
        public decimal? ValueUsed { get; set; }
        public bool? IsCusMaterial { get; set; }

        public virtual FabricMaterialTaskVM? Material { get; set; }
    }
}
