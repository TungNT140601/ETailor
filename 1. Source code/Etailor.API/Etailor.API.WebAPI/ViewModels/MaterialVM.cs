namespace Etailor.API.WebAPI.ViewModels
{
    public class MaterialVM
    {
        public string Id { get; set; }
        public string? MaterialCategoryId { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
        public decimal? Quantity { get; set; }
    }
}
