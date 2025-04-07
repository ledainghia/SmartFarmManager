
using Quartz;
using SmartFarmManager.Service.Interfaces;


namespace SmartFarmManager.API.HostedServices
{
    public class AppHostedService : IHostedService
    {
        private readonly IScheduler _scheduler;
        private readonly IQuartzService _quartzService;
        private readonly ILogger<AppHostedService> _logger;
        //private readonly IMqttService _mqttService;

        public AppHostedService(IScheduler scheduler, IQuartzService quartzService, ILogger<AppHostedService> logger)
        {
            _scheduler = scheduler;
            _quartzService = quartzService;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Jobs scheduled. Starting scheduler...");
            await _scheduler.Start(cancellationToken);
            _logger.LogInformation($"Scheduler started: {_scheduler.IsStarted}");
            _logger.LogInformation("Starting Quartz Scheduler...");
            await _quartzService.LoadBackgroundJobDefault(cancellationToken);
            
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Shutdown(cancellationToken);
            //await _mqttService.DisconnectBrokerAsync(cancellationToken);
        }
    }
}
