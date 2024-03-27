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
            RecurringJob.AddOrUpdate<ITaskService>("AutoCreateEmptyTaskProduct", x => x.AutoCreateEmptyTaskProduct(), Cron.Hourly(0));

            RecurringJob.AddOrUpdate("KeepServerAliveMethod", () => Ultils.KeepServerAlive(_wwwroot), "*/5 * * * *");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            RecurringJob.RemoveIfExists("AutoCreateEmptyTaskProduct");
            RecurringJob.RemoveIfExists("KeepServerAliveMethod");

            return Task.CompletedTask;
        }
    }
}
