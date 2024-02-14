using System;
using System.Collections.Generic;

namespace Etailor.API.Repository.EntityModels
{
    public partial class Discount
    {
        public Discount()
        {
            Orders = new HashSet<Order>();
        }

        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Code { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public float? DiscountPercent { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal? ConditionPriceMin { get; set; }
        public decimal? ConditionPriceMax { get; set; }
        public int? ConditionProductMin { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastestUpdatedTime { get; set; }
        public DateTime? InactiveTime { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
