using Etailor.API.Repository.StoreProcModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository.Interface.Dashboard
{
    public interface IOrderDashboardRepository : IGenericRepository<OrderDashboard>
    {
    }
    public interface IStaffWithTotalTaskRepository : IGenericRepository<StaffWithTotalTask>
    {
    }
}
