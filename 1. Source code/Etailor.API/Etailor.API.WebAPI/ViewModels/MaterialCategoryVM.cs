using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class MaterialCategoryVM
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public double? PricePerUnit { get; set; }
        public DateTime? CreatedTime { get; set; }
    }

    public class CreateMaterialCategoryVM
    {
        public string? Name { get; set; }
        public double? PricePerUnit { get; set; }
    }

    public class UpdateMaterialCategoryVM
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public double? PricePerUnit { get; set; }
    }

    public class MaterialCaterogyTaskDetailVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }
}
