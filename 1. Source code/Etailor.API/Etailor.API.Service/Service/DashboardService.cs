using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface.Dashboard;
using Etailor.API.Repository.StoreProcModels;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly IOrderDashboardRepository orderDashboardRepository;
        private readonly IStaffWithTotalTaskRepository staffWithTotalTaskRepository;

        public DashboardService(IOrderDashboardRepository orderDashboardRepository
            , IStaffWithTotalTaskRepository staffWithTotalTaskRepository)
        {
            this.orderDashboardRepository = orderDashboardRepository;
            this.staffWithTotalTaskRepository = staffWithTotalTaskRepository;
        }

        public object GetOrderDashboard(DateTime? date)
        {
            SqlParameter startDateParam = new SqlParameter("@StartDate", SqlDbType.DateTime);

            if (date != null)
            {
                startDateParam.Value = date;
            }
            else
            {
                startDateParam.Value = DateTime.UtcNow.AddHours(7);
            }

            var orderDashboards = orderDashboardRepository.GetStoreProcedure(StoreProcName.Get_Order_Dashboard, startDateParam);

            if (orderDashboards != null && orderDashboards.Any())
            {
                orderDashboards = orderDashboards.ToList();
            }

            var returnResult = new List<OrderDashboard>();

            for (int i = 0; i <= 7; i++)
            {
                if (orderDashboards == null || !orderDashboards.Any(x => x.Status == i))
                {
                    returnResult.Add(new OrderDashboard
                    {
                        Status = i,
                        Total = 0
                    });
                }
                else
                {
                    returnResult.Add(orderDashboards.First(x => x.Status == i));
                }
            }

            return returnResult.OrderBy(x => x.Status);
        }

        public async Task<object> GetStaffWithTotalTask()
        {
            var staffs = staffWithTotalTaskRepository.GetStoreProcedure(StoreProcName.Get_Staff_With_Total_Task);

            if (staffs != null && staffs.Any())
            {
                staffs = staffs.ToList();

                var tasks = new List<Task>();

                foreach (var staff in staffs)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        if (!string.IsNullOrWhiteSpace(staff.Avatar))
                        {
                            staff.Avatar = Ultils.GetUrlImage(staff.Avatar);
                        }
                        else
                        {
                            staff.Avatar = string.Empty;
                        }
                    }));
                }
                await Task.WhenAll(tasks);

                return staffs;
            }

            return null;
        }

        public int GetTotalOrder()
        {
            var thisMonth = orderDashboardRepository.GetStoreProcedure(StoreProcName.Get_Order_Dashboard, new SqlParameter("@StartDate", DateTime.UtcNow.AddHours(7)));
            if (thisMonth != null && thisMonth.Any(x => x.Status == 7))
            {
                return thisMonth.Single(x => x.Status == 7).Total.Value;
            }
            else
            {
                return 0;
            }
        }

        public double GetOrderRate()
        {
            var thisMonth = orderDashboardRepository.GetStoreProcedure(StoreProcName.Get_Order_Dashboard, new SqlParameter("@StartDate", DateTime.UtcNow.AddHours(7)));
            var preMonth = orderDashboardRepository.GetStoreProcedure(StoreProcName.Get_Order_Dashboard, new SqlParameter("@StartDate", DateTime.UtcNow.AddHours(7).AddHours(-1)));
            double totalThisMonth = 0;
            double totalPreMonth = 0;

            if (thisMonth != null && thisMonth.Any(x => x.Status == 7))
            {
                totalThisMonth = thisMonth.Single(x => x.Status == 7).Total.Value;
            }
            else
            {
                totalThisMonth = 0;
            }

            if (preMonth != null && preMonth.Any(x => x.Status == 7))
            {
                totalPreMonth = preMonth.Single(x => x.Status == 7).Total.Value;
            }
            else
            {
                totalPreMonth = 0;
            }

            if (totalPreMonth == 0 && totalThisMonth != 0)
            {
                return 1;
            }
            else if (totalPreMonth == 0 && totalThisMonth == 0)
            {
                return 0;
            }
            else
            {
                return (double)(totalThisMonth - totalPreMonth) / totalPreMonth;
            }
        }
        public decimal GetTotalOrderPrice()
        {
            var thisMonth = orderDashboardRepository.GetStoreProcedure(StoreProcName.Get_Order_Dashboard, new SqlParameter("@StartDate", DateTime.UtcNow.AddHours(7)));
            if (thisMonth != null && thisMonth.Any(x => x.Status == 7))
            {
                return thisMonth.Single(x => x.Status == 7).TotalPrice.Value;
            }
            else
            {
                return 0;
            }
        }

        public double GetOrderTotalPriceRate()
        {
            var thisMonth = orderDashboardRepository.GetStoreProcedure(StoreProcName.Get_Order_Dashboard, new SqlParameter("@StartDate", DateTime.UtcNow.AddHours(7)));
            var preMonth = orderDashboardRepository.GetStoreProcedure(StoreProcName.Get_Order_Dashboard, new SqlParameter("@StartDate", DateTime.UtcNow.AddHours(7).AddHours(-1)));
            decimal totalThisMonth = 0;
            decimal totalPreMonth = 0;

            if (thisMonth != null && thisMonth.Any(x => x.Status == 7))
            {
                totalThisMonth = thisMonth.Single(x => x.Status == 7).TotalPrice.Value;
            }
            else
            {
                totalThisMonth = 0;
            }

            if (preMonth != null && preMonth.Any(x => x.Status == 7))
            {
                totalPreMonth = preMonth.Single(x => x.Status == 7).TotalPrice.Value;
            }
            else
            {
                totalPreMonth = 0;
            }

            if (totalPreMonth == 0 && totalThisMonth != 0)
            {
                return 1;
            }
            else if (totalPreMonth == 0 && totalThisMonth == 0)
            {
                return 0;
            }
            else
            {
                return (double)(totalThisMonth - totalPreMonth) / (double)totalPreMonth;
            }
        }
    }
}
