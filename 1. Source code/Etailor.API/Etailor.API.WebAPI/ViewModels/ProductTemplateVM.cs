namespace Etailor.API.WebAPI.ViewModels
{
    public class ProductTemplateCreateVM
    {
        public string? Id { get; set; }
        public string? CategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public IFormFile? ThumbnailImageFile { get; set; }
        public List<IFormFile>? ImageFiles { get; set; }
        public List<IFormFile>? CollectionImageFiles { get; set; }
    }
    public class ProductTemplateALLVM
    {
        public string? Id { get; set; }
        public CategoryAllVM? Category { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? ThumbnailImage { get; set; }
        public string? Image { get; set; }
        public string? CollectionImage { get; set; }
        public string? UrlPath { get; set; }
    }
    public class ProductTemplateTaskDetailVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? ThumbnailImage { get; set; }
    }

}
