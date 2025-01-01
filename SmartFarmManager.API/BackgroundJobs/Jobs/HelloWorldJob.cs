using Quartz;

namespace SmartFarmManager.API.BackgroundJobs.Jobs
{
    public class HelloWorldJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"Hello World! Executed at: {DateTime.Now}");
            await Task.CompletedTask;
        }
    }
}
