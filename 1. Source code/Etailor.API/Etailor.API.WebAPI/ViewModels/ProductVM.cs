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
        public string? ProductTemplateName { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }
        public string? ReferenceProfileBodyId { get; set; }
        public string? ProfileBodyName { get; set; }
        public List<ProfileBodyDetailVM> ProfileBodyValue { get; set; }
        public string? FabricMaterialId { get; set; }
        public string? MaterialName { get; set; }
        public string? MaterialImage {  get; set; }
        public decimal? MaterialQuantity {  get; set; }
        public DateTime? CreatedTime { get; set; }

        public List<ProductStageDetailVM> ProductStages { get; set; }

        public List<ProductComponentDetailVM> ProductComponents { get; set; }  
    }

    public class ProductComponentDetailVM
    {
        public string ProductComponentId { get; set; }
        public string? ComponentId { get; set; }
        public string? ProductStageId { get; set; }
        public string? ProductComponentName { get; set; }
        public string? Image { get; set; }
        public ComponentDetailVM Component {  get; set; }

    }

    public class ComponentDetailVM
    {
        public string Id { get; set; }
        public string? ComponentTypeId { get; set; }
        public string? ProductTemplateId { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public int? Index { get; set; }
        public bool? Default { get; set; }
    }

    public class ProductStageDetailVM
    {
        public string ProductStageId { get; set; }
        public string? StaffId { get; set; }
        public string? TemplateStageId { get; set; }
        public string? TemplateStageName { get; set; }
        public int? TaskIndex { get; set; }
        public int? StageNum { get; set; }
        public DateTime? Deadline { get; set; }
        public int? Status { get; set; }
    }

    public class ProfileBodyDetailVM
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public decimal Value { get; set; }
    }
}
