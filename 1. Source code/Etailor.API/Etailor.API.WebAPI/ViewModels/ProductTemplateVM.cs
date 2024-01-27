namespace Etailor.API.WebAPI.ViewModels
{
    public class ProductTemplateALLVM
    {
        public string Id { get; set; } = null!;
        public string CategoryId { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? ThumbnailImage { get; set; }
        public string? Image { get; set; }
        public string? CollectionImage { get; set; }
        public string? UrlPath { get; set; }
    }

    public class CategoryAllTemplateVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }

        public IEnumerable<ProductTemplateALLVM>? ProductTemplates { get; set; }
    }
}
