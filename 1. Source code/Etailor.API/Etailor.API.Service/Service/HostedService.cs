using Etailor.API.Service.Interface;
using Etailor.API.Ultity;
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
    public class HostedService : IHostedService
    {
        private readonly string _wwwroot;
        public HostedService(IWebHostEnvironment webHostEnvironment)
        {
            this._wwwroot = webHostEnvironment.WebRootPath;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            RecurringJob.AddOrUpdate<IProductService>("AutoCreateEmptyTaskProduct", x => x.AutoCreateEmptyTaskProduct(), Cron.Hourly(0));

            RecurringJob.AddOrUpdate("KeepServerAliveMethod", () => Ultils.KeepServerAlive(_wwwroot), Cron.Minutely());

            //RecurringJob.AddOrUpdate<IProductStageService>("DemoRunMethod", x => x.SendDemoSchedule("* * * * * *"), "* * * * * *");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            RecurringJob.RemoveIfExists("AutoCreateEmptyTaskProduct");
            RecurringJob.RemoveIfExists("KeepServerAliveMethod");
            //RecurringJob.RemoveIfExists("DemoRunMethod");

            return Task.CompletedTask;
        }
    }
}
