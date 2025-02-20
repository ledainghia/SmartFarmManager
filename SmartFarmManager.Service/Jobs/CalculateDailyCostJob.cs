using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Jobs
{
    public class CalculateDailyCostJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CalculateDailyCostJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var costingService = scope.ServiceProvider.GetRequiredService<ICostingService>();

                Console.WriteLine($"Running daily cost calculation job at {DateTime.UtcNow}");

                await costingService.CalculateAndStoreDailyCostAsync();
            }
        }
    }
}
