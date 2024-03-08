using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Payment
    {
        public Payment()
        {
            RefundOfPayments = new HashSet<Payment>();
        }
        public string Id { get; set; } = null!;
        public string? OrderId { get; set; }
        public string? StaffCreateId { get; set; }
        public string? Platform { get; set; }
        public decimal? Amount { get; set; }
        public decimal? AmountAfterRefund { get; set; }
        public DateTime? PayTime { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? PayType { get; set; }
        public int? Status { get; set; }
        public string? PaymentRefundId { get; set; }
        public virtual Staff? StaffCreate { get; set; }
        public virtual Order? Order { get; set; }
        public virtual Payment? PaymentRefund { get; set; }
        public virtual ICollection<Payment>? RefundOfPayments { get; set; }
    }
}
