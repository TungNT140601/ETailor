namespace Etailor.API.WebAPI.ViewModels
{
    public class BodySizeVM
    {
        public string Id { get; set; }
        public string? BodyPart { get; set; }
        public int? BodyIndex { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public string? GuideVideoLink { get; set; }
        public decimal? MinValidValue { get; set; }
        public decimal? MaxValidValue { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }
    }
}
