using Etailor.API.Repository.DataAccess;
using Etailor.API.Repository.Interface.Dashboard;
using Etailor.API.Repository.Repository;
using Etailor.API.Repository.StoreProcModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Repository.Repository.Dashoard
{
    public class OrderDashoardRepository : GenericRepository<OrderDashboard>, IOrderDashboardRepository
    {
        public OrderDashoardRepository(ETailor_DBContext dBContext) : base(dBContext)
        {
        }
    }
    public class StaffWithTotalTaskRepository : GenericRepository<StaffWithTotalTask>, IStaffWithTotalTaskRepository
    {
        public StaffWithTotalTaskRepository(ETailor_DBContext dBContext) : base(dBContext)
        {
        }
    }
    public class FabricMaterialCommonUsedRepository : GenericRepository<FabricMaterialCommonUsed>, IFabricMaterialCommonUsedRepository
    {
        public FabricMaterialCommonUsedRepository(ETailor_DBContext dBContext) : base(dBContext)
        {
        }
    }
    public class TemplateDashboardRepository : GenericRepository<TemplateDashboard>, ITemplateDashboardRepository
    {
        public TemplateDashboardRepository(ETailor_DBContext dBContext) : base(dBContext)
        {
        }
    }
}
