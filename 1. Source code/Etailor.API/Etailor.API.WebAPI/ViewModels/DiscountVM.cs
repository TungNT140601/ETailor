namespace Etailor.API.WebAPI.ViewModels
{
    public class DiscountVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal? ConditionPriceMin { get; set; }
        public decimal? ConditionPriceMax { get; set; }
        public int? ConditionProductMin { get; set; }
    }

    public class DiscountCreateVM
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal? ConditionPriceMin { get; set; }
        public decimal? ConditionPriceMax { get; set; }
        public int? ConditionProductMin { get; set; }
    }
    public class DiscountOrderDetailVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
    }
}
