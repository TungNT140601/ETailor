using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class TemplateStageCreateVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int? StageNum { get; set; }
        public List<string>? ComponentTypeIds { get; set; }
    }
    public class TemplateStageAllVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int? StageNum { get; set; }
        public virtual List<ComponentStageVM>? ComponentStages { get; set; }
    }
    public class TemplateStageAllTaskVM
    {
        public string? Id { get; set; }
        public string? ProductTemplateId { get; set; }
        public string? TemplateStageId { get; set; }
        public string? Name { get; set; }
        public int? StageNum { get; set; }
    }
}
