namespace Etailor.API.WebAPI.ViewModels
{
    public class BlogVM
    {
        public string Id { get; set; }
        public string? Title { get; set; }
        public string? UrlPath { get; set; }
        public string? Content { get; set; }
        public string? Hastag { get; set; }
        public string? Thumbnail { get; set; }
    }

    public class CreateBlogVM
    {
        public string? Title { get; set; }
        //public string? UrlPath { get; set; }
        public string? Content { get; set; }
        public string? Hastag { get; set; }

        public IFormFile? Thumbnail { get; set; }
    }

    public class UpdateBlogVM
    {
        public string Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Hastag { get; set; }
        public IFormFile? Thumbnail { get; set; }
    }

    public class ListOfBlogVM
    {
        public string Id { get; set; }
        public string? Title { get; set; }
        public string? UrlPath { get; set; }
        public string? Content { get; set; }
        public string? Hastag { get; set; }
        public string? Thumbnail { get; set; }
        public DateTime? CreatedTime { get; set; }
    }
}
