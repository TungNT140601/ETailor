using Etailor.API.Repository.EntityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Interface
{
    public interface IDashboardService
    {
        object GetOrderDashboard(int? year, int? month);
        object GetOrderDashboardByYear(int? year);
        Task<object> GetStaffWithTotalTask();
        int GetTotalOrder(int? year, int? month);
        double GetOrderRate(int? year, int? month);
        decimal GetTotalOrderPrice(int? year, int? month);
        double GetOrderTotalPriceRate(int? year, int? month);
    }
}
