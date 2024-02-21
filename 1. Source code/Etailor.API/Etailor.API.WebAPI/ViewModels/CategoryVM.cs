namespace Etailor.API.WebAPI.ViewModels
{
    public class CategoryVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public List<ComponentTypeFormVM>? ComponentTypes { get; set; }
    }
    public class CategoryAllVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }
    public class CategoryAllTemplateVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }

        public IEnumerable<ProductTemplateALLVM>? ProductTemplates { get; set; }
    }
}
