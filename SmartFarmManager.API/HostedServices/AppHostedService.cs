
using Quartz;
using SmartFarmManager.Service.Interfaces;


namespace SmartFarmManager.API.HostedServices
{
    public class AppHostedService : IHostedService
    {
        private readonly IScheduler _scheduler;
        private readonly IQuartzService _quartzService;
        //private readonly IMqttService _mqttService;

        public AppHostedService(IScheduler scheduler, IQuartzService quartzService)
        {
            _scheduler = scheduler;
            _quartzService = quartzService;
           
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
          //  await _mqttService.ConnectBrokerAsync(cancellationToken);
            await _quartzService.LoadBackgroundJobDefault(cancellationToken);

            // Khởi động scheduler
            await _scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Shutdown(cancellationToken);
            //await _mqttService.DisconnectBrokerAsync(cancellationToken);
        }
    }
}
