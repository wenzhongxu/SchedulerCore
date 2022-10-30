using SchedulerCore.Host.Managers;

namespace SchedulerCore.Host.Services
{
    public class HostedService : IHostedService
    {
        private readonly SchedulerCenter _schedulerCenter;

        public HostedService(SchedulerCenter schedulerCenter)
        {
            _schedulerCenter = schedulerCenter ?? throw new ArgumentNullException(nameof(schedulerCenter));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _schedulerCenter.StartSchedulerAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
