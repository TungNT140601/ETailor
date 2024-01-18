using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Transaction
    {
        public string Id { get; set; } = null!;
        public string? OrderId { get; set; }
        public string? Platform { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public DateTime? TransactionTime { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? Status { get; set; }

        public virtual Order? Order { get; set; }
    }
}
