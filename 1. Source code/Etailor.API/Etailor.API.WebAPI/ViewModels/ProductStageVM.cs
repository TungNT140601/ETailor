using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class ProductStageVM
    {
        public string Id { get; set; } = null!;
        public string? StaffId { get; set; }
        public string? TemplateStageId { get; set; }
        public string? ProductId { get; set; }
        public string? StageName { get; set; }
        public int? StageNum { get; set; }
        public int? TaskIndex { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? Deadline { get; set; }
        public int? Status { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ProductStagesNeedForTask
    {
        public string? Id { get; set; }
        public string? StaffId { get; set; }
        public string? TemplateStageId { get; set; }
        public string? ProductId { get; set; }
        public int? StageNum { get; set; }
        public string? StageName { get; set; }
        public DateTime? Deadline { get; set; }
        public int? Status { get; set; }
    }
    public class ProductStageTaskDetailVM
    {
        public string? Id { get; set; }
        public string? StaffId { get; set; }
        public string? TemplateStageId { get; set; }
        public string? ProductId { get; set; }
        public string? StageName { get; set; }
        public int? StageNum { get; set; }
        public int? TaskIndex { get; set; }
        public string? EvidenceImage { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? Deadline { get; set; }
        public int? Status { get; set; }

        public virtual ICollection<ProductComponentTaskDetailVM>? ProductComponents { get; set; }
        public virtual ICollection<ProductStageMaterialOrderVM>? ProductStageMaterials { get; set; }
    }
}
