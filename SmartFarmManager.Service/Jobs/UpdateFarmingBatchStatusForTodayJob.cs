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
    public class UpdateFarmingBatchStatusForTodayJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UpdateFarmingBatchStatusForTodayJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope()) // Tạo scope mới để resolve dịch vụ
            {
                var farmingBatchService = scope.ServiceProvider.GetRequiredService<IFarmingBatchService>(); 

                Console.WriteLine($"Checking and updating farming batch statuses for today... {DateTimeUtils.GetServerTimeInVietnamTime()}");

                // Gọi hàm để kiểm tra và cập nhật trạng thái từ Planning sang Active
                await farmingBatchService.RunUpdateFarmingBatchesStatusAsync();
            }
        }
    }
}
