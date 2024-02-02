namespace Etailor.API.WebAPI.ViewModels
{
    public class BlogVM
    {
        public string Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
    }

    public class ListOfBlogVM
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
    }
}
