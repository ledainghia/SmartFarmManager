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
    public class UpdateGrowthStagesStatusJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UpdateGrowthStagesStatusJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope()) // Tạo scope mới để resolve dịch vụ
            {
                var growthStageService = scope.ServiceProvider.GetRequiredService<IGrowthStageService>();

                Console.WriteLine($"Checking and updating growth stage statuses for today... {DateTimeUtils.GetServerTimeInVietnamTime()}");

                // Gọi hàm để kiểm tra và cập nhật trạng thái từ Planning sang Active
                await growthStageService.UpdateGrowthStagesStatusAsync();
            }
        }
    }
}
