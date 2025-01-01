using Quartz;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.BackgroundJobs.Jobs
{
    public class UpdateEveningTaskStatusesJob : IJob
    {
        private readonly ITaskService _taskService;

        public UpdateEveningTaskStatusesJob(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine($"Running UpdateEveningTaskStatusesJob at: {DateTime.UtcNow}");
                var result = await _taskService.UpdateEveningTaskStatusesAsync();
                Console.WriteLine($"UpdateEveningTaskStatusesJob completed with result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateEveningTaskStatusesJob: {ex.Message}");
            }
        }
    }
}
