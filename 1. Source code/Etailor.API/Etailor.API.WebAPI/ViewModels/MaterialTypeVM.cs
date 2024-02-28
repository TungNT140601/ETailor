using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class MaterialTypeVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Unit { get; set; }

        //public virtual ICollection<MaterialCategory> MaterialCategories { get; set; }
    }
    public class MaterialTypeAllVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Unit { get; set; }
    }
}
