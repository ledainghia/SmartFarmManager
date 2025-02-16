using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Jobs
{
    public class UpdateTaskStatusesJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UpdateTaskStatusesJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope()) // ✅ Tạo scope mới
            {
                try
                {
                    var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>(); // ✅ Resolve Scoped Service

                    Console.WriteLine($"Running UpdateTaskStatusesJob at: {DateTimeUtils.GetServerTimeInVietnamTime()}");
                    var result = await taskService.UpdateAllTaskStatusesAsync();
                    Console.WriteLine($"UpdateTaskStatusesJob completed with result: {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in UpdateTaskStatusesJob: {ex.Message}");
                }
            }
        }
    }
}
