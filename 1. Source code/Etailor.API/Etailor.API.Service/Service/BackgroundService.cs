using Etailor.API.Repository.Interface;
using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
using Etailor.API.Ultity.CommonValue;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Service.Service
{
    public class BackgroundService : IBackgroundService
    {
        private readonly IStaffRepository staffRepository;
        private readonly string _wwwrootPath;
        public BackgroundService(IStaffRepository staffRepository, IWebHostEnvironment webHost)
        {
            this.staffRepository = staffRepository;
            this._wwwrootPath = webHost.WebRootPath;
        }

        public void CheckAvatarStaff()
        {

        }
        public void CheckAvatarCustomer()
        {

        }

        public void StartSchedule(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                AutoCreateEmptyTaskProductSchedule(true);
                KeepServerAliveMethodSchedule(true);
            }
            else
            {
                if (id == "DemoRunMethod")
                {
                }
                else if (id == "AutoCreateEmptyTaskProduct")
                {
                    AutoCreateEmptyTaskProductSchedule(true);
                }
                else if (id == "KeepServerAliveMethod")
                {
                    KeepServerAliveMethodSchedule(true);
                }
            }
        }
        public void StopSchedule(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                AutoCreateEmptyTaskProductSchedule(false);
                KeepServerAliveMethodSchedule(false);
            }
            else
            {
                if (id == "DemoRunMethod")
                {
                }
                else if (id == "AutoCreateEmptyTaskProduct")
                {
                    AutoCreateEmptyTaskProductSchedule(false);
                }
                else if (id == "KeepServerAliveMethod")
                {
                    KeepServerAliveMethodSchedule(false);
                }
            }
        }

        private void AutoCreateEmptyTaskProductSchedule(bool startOrStop)
        {
            if (startOrStop)
            {
                RecurringJob.AddOrUpdate<ITaskService>("AutoCreateEmptyTaskProduct", x => x.AutoCreateEmptyTaskProduct(), Cron.Hourly(0));
            }
            else
            {
                RecurringJob.RemoveIfExists("AutoCreateEmptyTaskProduct");
            }
        }

        private void KeepServerAliveMethodSchedule(bool startOrStop)
        {
            if (startOrStop)
            {
                RecurringJob.AddOrUpdate("KeepServerAliveMethod", () => Ultils.KeepServerAlive(_wwwrootPath), "*/5 * * * *");
            }
            else
            {
                RecurringJob.RemoveIfExists("KeepServerAliveMethod");
            }
        }
    }
}
