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
        public bool? IsCusMaterial { get; set; }
        public double? MaterialQuantity { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }
        public List<ComponentTypeOrderVM>? ComponentTypeOrders { get; set; }
    }
}
