using Etailor.API.Repository.EntityModels;
using Etailor.API.Repository.Interface.Dashboard;
using Etailor.API.Repository.StoreProcModels;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Hangfire.MemoryStorage.Database;
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
        private readonly IFabricMaterialCommonUsedRepository fabricMaterialCommonUsedRepository;

        public DashboardService(IOrderDashboardRepository orderDashboardRepository
            , IStaffWithTotalTaskRepository staffWithTotalTaskRepository
            , IFabricMaterialCommonUsedRepository fabricMaterialCommonUsedRepository)
        {
            this.orderDashboardRepository = orderDashboardRepository;
            this.staffWithTotalTaskRepository = staffWithTotalTaskRepository;
            this.fabricMaterialCommonUsedRepository = fabricMaterialCommonUsedRepository;
        }

        public object GetOrderDashboard(int? year, int? month)
        {
            SqlParameter startDateParam = new SqlParameter("@StartDate", SqlDbType.DateTime);

            if (year == null || year > DateTime.UtcNow.AddHours(7).Year)
            {
                year = DateTime.UtcNow.AddHours(7).Year;
            }
            if (month == null || (month > DateTime.UtcNow.AddHours(7).Month && year >= DateTime.UtcNow.AddHours(7).Year))
            {
                month = DateTime.UtcNow.AddHours(7).Month;
            }

            startDateParam.Value = new DateTime(year.Value, month.Value, 1);

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

        public object GetOrderDashboardByYear(int? year)
        {
            if (year == null)
            {
                year = DateTime.UtcNow.AddHours(7).Year;
            }

            var returnResult = new List<OrderDashboardMonth>();

            for (int month = 1; month <= 12; month++)
            {
                var date = new DateTime(year.Value, month, 1);

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

                if (orderDashboards != null && orderDashboards.Any(x => x.Status == 7 || x.Status == 0))
                {
                    orderDashboards = orderDashboards.ToList();

                    var done = orderDashboards.SingleOrDefault(x => x.Status == 7);
                    var cancel = orderDashboards.SingleOrDefault(x => x.Status == 0);
                    returnResult.Add(new OrderDashboardMonth
                    {
                        Year = year.Value,
                        Month = month,
                        OrderCancel = cancel != null ? cancel : new OrderDashboard()
                        {
                            Status = 0,
                            Total = 0,
                            TotalPrice = 0
                        },
                        OrderDone = done != null ? done : new OrderDashboard()
                        {
                            Status = 7,
                            Total = 0,
                            TotalPrice = 0
                        }
                    });
                }
                else
                {
                    returnResult.Add(new OrderDashboardMonth
                    {
                        Year = year.Value,
                        Month = month,
                        OrderCancel = new OrderDashboard()
                        {
                            Status = 0,
                            Total = 0,
                            TotalPrice = 0
                        },
                        OrderDone = new OrderDashboard()
                        {
                            Status = 7,
                            Total = 0,
                            TotalPrice = 0
                        }
                    });
                }
            }

            return returnResult.OrderBy(x => x.Month);
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
        public int GetTotalOrder(int? year, int? month)
        {
            if (year == null || year > DateTime.UtcNow.AddHours(7).Year)
            {
                year = DateTime.UtcNow.AddHours(7).Year;
            }
            if (month == null || (month > DateTime.UtcNow.AddHours(7).Month && year >= DateTime.UtcNow.AddHours(7).Year))
            {
                month = DateTime.UtcNow.AddHours(7).Month;
            }

            var date = new DateTime(year.Value, month.Value, 1);

            var thisMonth = orderDashboardRepository.GetStoreProcedure(StoreProcName.Get_Order_Dashboard, new SqlParameter("@StartDate", date));
            if (thisMonth != null && thisMonth.Any(x => x.Status == 7))
            {
                return thisMonth.Single(x => x.Status == 7).Total.Value;
            }
            else
            {
                return 0;
            }
        }
        public double GetOrderRate(int? year, int? month)
        {
            if (year == null || year > DateTime.UtcNow.AddHours(7).Year)
            {
                year = DateTime.UtcNow.AddHours(7).Year;
            }
            if (month == null || (month > DateTime.UtcNow.AddHours(7).Month && year >= DateTime.UtcNow.AddHours(7).Year))
            {
                month = DateTime.UtcNow.AddHours(7).Month;
            }

            var date = new DateTime(year.Value, month.Value, 1);
            var preDate = date.AddMonths(-1);

            var thisMonth = orderDashboardRepository.GetStoreProcedure(StoreProcName.Get_Order_Dashboard, new SqlParameter("@StartDate", date));
            var preMonth = orderDashboardRepository.GetStoreProcedure(StoreProcName.Get_Order_Dashboard, new SqlParameter("@StartDate", preDate));
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
        public decimal GetTotalOrderPrice(int? year, int? month)
        {
            if (year == null || year > DateTime.UtcNow.AddHours(7).Year)
            {
                year = DateTime.UtcNow.AddHours(7).Year;
            }
            if (month == null || (month > DateTime.UtcNow.AddHours(7).Month && year >= DateTime.UtcNow.AddHours(7).Year))
            {
                month = DateTime.UtcNow.AddHours(7).Month;
            }

            var date = new DateTime(year.Value, month.Value, 1);

            var thisMonth = orderDashboardRepository.GetStoreProcedure(StoreProcName.Get_Order_Dashboard, new SqlParameter("@StartDate", date));
            if (thisMonth != null && thisMonth.Any(x => x.Status == 7))
            {
                return thisMonth.Single(x => x.Status == 7).TotalPrice.Value;
            }
            else
            {
                return 0;
            }
        }
        public double GetOrderTotalPriceRate(int? year, int? month)
        {
            if (year == null || year > DateTime.UtcNow.AddHours(7).Year)
            {
                year = DateTime.UtcNow.AddHours(7).Year;
            }
            if (month == null || (month > DateTime.UtcNow.AddHours(7).Month && year >= DateTime.UtcNow.AddHours(7).Year))
            {
                month = DateTime.UtcNow.AddHours(7).Month;
            }
            var date = new DateTime(year.Value, month.Value, 1);
            var preDate = date.AddMonths(-1);

            var thisMonth = orderDashboardRepository.GetStoreProcedure(StoreProcName.Get_Order_Dashboard, new SqlParameter("@StartDate", date));
            var preMonth = orderDashboardRepository.GetStoreProcedure(StoreProcName.Get_Order_Dashboard, new SqlParameter("@StartDate", preDate));
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

        public async Task<List<FabricMaterialCommonUsed>> GetFabricMaterialCommonUsedByMonth(int? year, int? month)
        {
            if (year == null || year > DateTime.UtcNow.AddHours(7).Year)
            {
                year = DateTime.UtcNow.AddHours(7).Year;
            }
            if (month == null || (month > DateTime.UtcNow.AddHours(7).Month && year >= DateTime.UtcNow.AddHours(7).Year))
            {
                month = DateTime.UtcNow.AddHours(7).Month;
            }
            var date = new DateTime(year.Value, month.Value, 1);

            var monthData = fabricMaterialCommonUsedRepository.GetStoreProcedure(StoreProcName.Get_Total_Fabric_Material_Common_Used, new SqlParameter("@StartDate", date));

            if (monthData != null && monthData.Any())
            {
                monthData = monthData.ToList();
                var tasks = new List<Task>();
                foreach (var item in monthData)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        item.Image = Ultils.GetUrlImage(item.Image);
                    }));
                }
                await Task.WhenAll(tasks);

                return monthData.ToList();
            }

            return null;
        }
    }
}
