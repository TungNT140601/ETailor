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

        public List<ProductTemplateALLVM>? ProductTemplates { get; set; }
    }
    public class CategoryAllTaskVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int? TotalTask { get; set; }
        public List<ProductTemplateAllTaskVM>? ProductTemplates { get; set; }
    }
}
