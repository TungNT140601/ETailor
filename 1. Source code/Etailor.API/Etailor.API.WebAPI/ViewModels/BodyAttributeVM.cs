namespace Etailor.API.WebAPI.ViewModels
{
    public class BodyAttributeVM
    {
        public string Id { get; set; }
        public string? ProfileBodyId { get; set; }
        public string? BodySizeId { get; set; }
        public decimal? Value { get; set; }
    }

    public class CreateBodyAttributeVM
    {
        public string Id { get; set; } 
        public string? ProfileBodyId { get; set; }
        public string? BodySizeId { get; set; }
        public decimal? Value { get; set; }
    }

    public class ValueBodyAttributeVM
    {
        public string? BodySizeId { get; set; }
        public decimal? Value { get; set; }
    }
}
