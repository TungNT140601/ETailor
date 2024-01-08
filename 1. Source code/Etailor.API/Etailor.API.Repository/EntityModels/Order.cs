using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
            OrderMaterials = new HashSet<OrderMaterial>();
            Transactions = new HashSet<Transaction>();
        }

        public string Id { get; set; } = null!;
        public string? CustomerId { get; set; }
        public int? TotalProduct { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? DiscountId { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string? DiscountCode { get; set; }
        public decimal? AfterDiscountPrice { get; set; }
        public bool? PayDeposit { get; set; }
        public decimal? Deposit { get; set; }
        public decimal? PaidMoney { get; set; }
        public decimal? UnPaidMoney { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? ApproveTime { get; set; }
        public string? Approver { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? BuyTime { get; set; }
        public bool? IsBuy { get; set; }
        public DateTime? CancelTime { get; set; }
        public bool? IsCancel { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Staff? ApproverNavigation { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual Discount? Discount { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<OrderMaterial> OrderMaterials { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
