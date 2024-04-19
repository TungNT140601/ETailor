using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository.StoreProcModels
{
    public class OrderDashboard
    {
        public int? Status { get; set; }
        public int? Total { get; set; }
        public decimal? TotalPrice { get; set; }
    }
    public class OrderDashboardMonth
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
        public OrderDashboard? OrderDone { get; set; }
        public OrderDashboard? OrderCancel { get; set; }
    }
}
