namespace Etailor.API.WebAPI.ViewModels
{
    public class ComponentVM
    {
        public string? Id { get; set; }
        //public string? ComponentTypeId { get; set; }
        //public string? ProductTemplateId { get; set; }
        public string? Name { get; set; }
        //public string? Image { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
