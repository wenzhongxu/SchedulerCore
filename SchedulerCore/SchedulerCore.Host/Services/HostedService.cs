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
        private readonly SchedulerManager _schedulerManager;

        public HostedService(SchedulerManager schedulerManager)
        {
            _schedulerManager = schedulerManager ?? throw new ArgumentNullException(nameof(schedulerManager));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _schedulerManager.StartScheduleAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
