using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class OrderDetail
    {
        public string Id { get; set; } = null!;
        public string? OrderId { get; set; }
        public string? ProductId { get; set; }
        public string? ProfileBodyAttributeId { get; set; }
        public string? Status { get; set; }
        public decimal? StatusPercent { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Order? Order { get; set; }
        public virtual Product? Product { get; set; }
        public virtual ProfileBodyAttribute? ProfileBodyAttribute { get; set; }
    }
}
