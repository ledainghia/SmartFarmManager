using Quartz;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.BackgroundJobs.Jobs
{
    public class UpdateTaskStatusesJob : IJob
    {
        private readonly ITaskService _taskService;

        public UpdateTaskStatusesJob(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine($"Running UpdateTaskStatusesJob at: {DateTime.UtcNow}");
                var result = await _taskService.UpdateAllTaskStatusesAsync();
                Console.WriteLine($"UpdateTaskStatusesJob completed with result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateTaskStatusesJob: {ex.Message}");
            }
        }
    }
}
