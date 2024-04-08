using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class ProductBodySizeTaskDetailVM
    {
        public string? Id { get; set; }
        public decimal? Value { get; set; }
        public virtual BodySizeTaskDetailVM? BodySize { get; set; }
    }
}
