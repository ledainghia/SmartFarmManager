using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Jobs
{
    public class GenerateTasksForTomorrowJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public GenerateTasksForTomorrowJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope()) // ✅ Tạo scope mới
            {
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>(); // ✅ Resolve ITaskService đúng cách

                Console.WriteLine($"Generating tasks for tomorrow... {DateTime.Now}");

                await taskService.GenerateTasksForTomorrowAsync();
                await taskService.GenerateTreatmentTasksAsyncV2();
            }
        }
    }
}
