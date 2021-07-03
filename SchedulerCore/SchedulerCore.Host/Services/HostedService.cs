using Microsoft.Extensions.Hosting;
using SchedulerCore.Host.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchedulerCore.Host.Services
{
    public class HostedService : IHostedService
    {
        //private readonly SchedulerManager _schedulerManager;
        private readonly SchedulerCenter _schedulerCenter;

        public HostedService(SchedulerCenter schedulerCenter)
        {
            _schedulerCenter = schedulerCenter ?? throw new ArgumentNullException(nameof(schedulerCenter));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _schedulerCenter.StartScheduleAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
