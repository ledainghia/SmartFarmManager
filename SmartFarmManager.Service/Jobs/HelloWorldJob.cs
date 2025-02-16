using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.Service.Jobs
{
    public class HelloWorldJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public HelloWorldJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope()) // Tạo scope mới
            {
                var medicationService = scope.ServiceProvider.GetRequiredService<IMedicationService>();
                var medications = await medicationService.GetAllMedicationsAsync();

                Console.WriteLine($"Hello World! Executed at: {DateTime.Now}");
                Console.WriteLine($"Medication Count: {medications.FirstOrDefault().Name}");
            }

            await Task.CompletedTask;
        }
    }
}
