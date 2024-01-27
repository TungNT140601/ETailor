namespace Etailor.API.WebAPI.ViewModels
{
    public class TemplateStageCreateVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int? StageNum { get; set; }
        public List<string>? ComponentTypeIds { get; set; }
    }
}
