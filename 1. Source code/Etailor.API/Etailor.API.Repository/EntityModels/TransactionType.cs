using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class TransactionType
    {
        public TransactionType()
        {
            Transactions = new HashSet<Transaction>();
        }

        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
