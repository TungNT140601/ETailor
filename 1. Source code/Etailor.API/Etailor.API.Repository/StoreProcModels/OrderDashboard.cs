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
}
