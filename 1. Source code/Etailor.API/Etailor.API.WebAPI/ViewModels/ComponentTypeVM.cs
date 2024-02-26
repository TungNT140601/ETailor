namespace Etailor.API.WebAPI.ViewModels
{
    public class ComponentTypeFormVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }
    public class ComponentTypeVM
    {
        public string? Id { get; set; }
        public string? CategoryId { get; set; }
        public string? Name { get; set; }
        public CategoryVM? Category { get; set; }
    }
    public class ComponentTypeTemplateVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public List<ComponentVM>? Components { get; set; }
    }
    public class ComponentTypeOrderVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public List<ComponentVM>? Components { get; set; }
    }
}
