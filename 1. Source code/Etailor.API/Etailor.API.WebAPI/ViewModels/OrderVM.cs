namespace Etailor.API.WebAPI.ViewModels
{
    public class OrderVM
    {
        public string Id { get; set; } = null!;
        public string? CustomerId { get; set; }
        public string? CreaterId { get; set; }
        public string? DiscountId { get; set; }
        public int? TotalProduct { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string? DiscountCode { get; set; }
        public decimal? AfterDiscountPrice { get; set; }
        public bool? PayDeposit { get; set; }
        public decimal? Deposit { get; set; }
        public decimal? PaidMoney { get; set; }
        public decimal? UnPaidMoney { get; set; }
        public int? Status { get; set; }
    }


}
