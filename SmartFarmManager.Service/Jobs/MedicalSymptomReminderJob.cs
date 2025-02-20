using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Jobs
{
    public class MedicalSymptomReminderJob : IJob
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MedicalSymptomReminderJob(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope()) 
            {
                var medicalSymptomService = scope.ServiceProvider.GetRequiredService<IMedicalSymptomService>();

                Console.WriteLine("Job Execution Started");

                // Lấy MedicalSymptomId từ JobDataMap
                var medicalSymptomId = context.JobDetail.JobDataMap.GetString("MedicalSymptomId");

                if (Guid.TryParse(medicalSymptomId, out var symptomId))
                {
                    // Gọi method xử lý trong MedicalSymptomService
                    await medicalSymptomService.ProcessMedicalSymptomReminderAsync(symptomId);
                    Console.WriteLine($"Processed MedicalSymptomReminder for ID: {symptomId}");
                }
                else
                {
                    Console.WriteLine("Invalid MedicalSymptomId");
                }
            }
        }
    }
}
