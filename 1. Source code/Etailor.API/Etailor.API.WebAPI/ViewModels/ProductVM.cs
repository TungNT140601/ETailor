namespace Etailor.API.WebAPI.ViewModels
{
    public class ProductVM
    {
        public string Id { get; set; }
        public string? OrderId { get; set; }
        public string? ProductTemplateId { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }
        public int? Status { get; set; }
        public string? EvidenceImage { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }
    }
}
