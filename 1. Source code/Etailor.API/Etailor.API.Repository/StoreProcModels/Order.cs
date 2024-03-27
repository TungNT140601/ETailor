using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.StoreProcModels
{
    public partial class Order
    {
        public Order()
        {
            Products = new HashSet<Product>();
        }

        public string OrderId { get; set; } = null!;
        public string? CustomerId { get; set; }
        public string? CusName { get; set; }
        public string? CusPhone { get; set; }
        public string? CusEmail { get; set; }
        public string? CusAddress { get; set; }
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
        public int? OrderStatus { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
