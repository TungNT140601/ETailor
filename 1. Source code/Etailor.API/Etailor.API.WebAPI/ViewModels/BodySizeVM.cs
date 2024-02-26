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
    }

    public class CreateUpdateBodySizeVM
    {
        public string Id { get; set; }
        public string? BodyPart { get; set; }
        public int? BodyIndex { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public string? GuideVideoLink { get; set; }
        public decimal? MinValidValue { get; set; }
        public decimal? MaxValidValue { get; set; }
    }

    public class CreateBodySizeVM
    {
        public string? BodyPart { get; set; }
        public int? BodyIndex { get; set; }
        public string? Name { get; set; }
        public IFormFile? Image { get; set; }
        public string? GuideVideoLink { get; set; }
        public decimal? MinValidValue { get; set; }
        public decimal? MaxValidValue { get; set; }
    }
}
