using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class ProductVM
    {
        public string? Id { get; set; }
        public string? OrderId { get; set; }
        public string? ProductTemplateId { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }
        public int? Status { get; set; }
        public string? EvidenceImage { get; set; }
        public DateTime? FinishTime { get; set; }
    }
    public class ProductOrderVM
    {
        public string? Id { get; set; }
        public string? OrderId { get; set; }
        public string? MaterialId { get; set; }
        public string? ProfileId { get; set; }
        public string? ProductTemplateId { get; set; }
        public bool? IsCusMaterial { get; set; }
        public double? MaterialQuantity { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }
        public List<ProductComponentOrderVM>? ProductComponents { get; set; }
    }
    public class ProductDetailOrderVM
    {
        public string? Id { get; set; }
        public string? OrderId { get; set; }
        public string? MaterialId { get; set; }
        public string? ProfileId { get; set; }
        public string? ProductTemplateId { get; set; }
        public string? ProductTemplateName { get; set; }
        public string? ProductTemplateImage { get; set; }
        public bool? IsCusMaterial { get; set; }
        public double? MaterialQuantity { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }
        public List<ComponentTypeOrderVM>? ComponentTypeOrders { get; set; }
    }
    public class ProductListOrderDetailVM
    {
        public string? Id { get; set; }
        public string? OrderId { get; set; }
        public string? MaterialId { get; set; }
        public string? ProductTemplateId { get; set; }
        public string? TemplateName { get; set; }
        public string? TemplateThumnailImage { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }
        public decimal? Price { get; set; }
        public int? Status { get; set; }
    }

    public class TaskListVM
    {
        public string Id { get; set; }
        public string? OrderId { get; set; }
        public string? ProductTemplateId { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }
        public decimal? Price { get; set; }
        public int? Status { get; set; }
        public string? ReferenceProfileBodyId { get; set; }
        public string? FabricMaterialId { get; set; }
        public string? StaffMakerId { get; set; }
        public int? Index { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? CreatedTime { get; set; }
    }

    public class TaskListByStaffVM
    {
        public string? Id { get; set; }
        public string? OrderId { get; set; }
        public string? ProductTemplateId { get; set; }
        public string? Name { get; set; }
        public int? Status { get; set; }
        public string? FabricMaterialId { get; set; }
        public int? Index { get; set; }
    }

    public class TaskDetailByStaffVM
    {
        public string? Id { get; set; }
        public string? OrderId { get; set; }
        public string? ProductTemplateId { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }
        public int? Status { get; set; }
        public string? EvidenceImage { get; set; }
        public string? ReferenceProfileBodyId { get; set; }
        public string? FabricMaterialId { get; set; }
        public int? Index { get; set; }
        public DateTime? FinishTime { get; set; }
        public FabricMaterialVM? FabricMaterial { get; set; }
        public ProductTemplateTaskDetailVM? ProductTemplate { get; set; }
        public ICollection<ProductBodySizeTaskDetailVM>? ProductBodySizes { get; set; }
        public ICollection<ProductStageTaskDetailVM>? ProductStages { get; set; }
    }

    public class ProfileBodyDetailVM
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public decimal Value { get; set; }
    }
}
