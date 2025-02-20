using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Jobs
{
    public class UpdateEveningTaskStatusesJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UpdateEveningTaskStatusesJob(IServiceScopeFactory serviceScopeFactory)
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

                    Console.WriteLine($"Running UpdateEveningTaskStatusesJob at: {DateTime.UtcNow}");
                    var result = await taskService.UpdateEveningTaskStatusesAsync();
                    Console.WriteLine($"UpdateEveningTaskStatusesJob completed with result: {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in UpdateEveningTaskStatusesJob: {ex.Message}");
                }
            }
        }
    }
}
