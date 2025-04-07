using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Jobs
{
    public class CheckAndNotifyAdminForUpcomingFarmingBatchesJob: IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CheckAndNotifyAdminForUpcomingFarmingBatchesJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope()) // Tạo scope mới để resolve dịch vụ
            {
                var farmingBatchService = scope.ServiceProvider.GetRequiredService<IFarmingBatchService>();

                Console.WriteLine($"Job started at: {DateTimeUtils.GetServerTimeInVietnamTime()}");
                await farmingBatchService.CheckAndNotifyAdminForUpcomingFarmingBatchesAsync();
                Console.WriteLine($"Job completed  at: {DateTimeUtils.GetServerTimeInVietnamTime()}");
            }
        }
    }
}
