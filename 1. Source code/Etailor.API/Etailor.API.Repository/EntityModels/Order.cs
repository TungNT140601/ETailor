using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Order
    {
        public Order()
        {
            OrderMaterials = new HashSet<OrderMaterial>();
            Payments = new HashSet<Payment>();
            Products = new HashSet<Product>();
            Chats = new HashSet<Chat>();
        }

        public string Id { get; set; } = null!;
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
        public int? Status { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? PlannedTime { get; set; }
        public DateTime? ApproveTime { get; set; }
        public DateTime? CancelTime { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual Staff? Creater { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual Discount? Discount { get; set; }
        public virtual ICollection<Chat> Chats { get; set; }
        public virtual ICollection<OrderMaterial> OrderMaterials { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
