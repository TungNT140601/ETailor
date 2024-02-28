using Etailor.API.Repository.EntityModels;

namespace Etailor.API.WebAPI.ViewModels
{
    public class OrderVM
    {

        public string Id { get; set; } = null!;
        public DateTime? CreatedTime { get; set; }
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
    public class OrderCreateVM
    {
        public string? Id { get; set; }
        public string? CustomerId { get; set; }
    }

    public class GetOrderVM
    {
        public string Id { get; set; } = null!;
        public DateTime? CreatedTime { get; set; }
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
        public string? ThumbnailImage { get; set; }
    }

    public class OrderByCustomerVM
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? ThumbnailImage { get; set; }
        public int? TotalProduct { get; set; }
        public int? Status { get; set; }
        public decimal? AfterDiscountPrice { get; set; }
        public DateTime? CreatedTime { get; set; }
    }
    public class OrderDetailVM
    {
        public string? Id { get; set; }
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
        public DiscountVM? Discount { get; set; }
        //public virtual ICollection<Payment> Payments { get; set; }
        public List<ProductListOrderDetailVM>? Products { get; set; }
    }
}
