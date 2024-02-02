using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class ComponentStageVM
    {
        public string? Id { get; set; }
        public string? ComponentTypeId { get; set; }
        public string? TemplateStageId { get; set; }

        public virtual ComponentTypeVM? ComponentType { get; set; }
    }
}
