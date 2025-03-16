using Quartz;
using SmartFarmManager.Service.Configuration;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class QuartzService : IQuartzService
    {
        private readonly IScheduler _scheduler;
        private readonly SystemConfigurationService _systemConfigurationService;

        public QuartzService(IScheduler scheduler,SystemConfigurationService systemConfigurationService)
        {
            _scheduler = scheduler;
            _systemConfigurationService = systemConfigurationService;
        }

        public async Task LoadBackgroundJobDefault(CancellationToken cancellationToken)
        {
            var serverTimeZone = TimeZoneInfo.Local;

            if (!_scheduler.IsStarted)
            {
                await _scheduler.Start();
                Console.WriteLine("Scheduler started manually.");
            }
            // Danh sách các jobs mặc định
            await ScheduleJob<Jobs.GenerateTasksForTomorrowJob>("GenerateTasksForTomorrowJob", "0 0 1 * * ?", serverTimeZone, cancellationToken);
            await ScheduleJob<Jobs.UpdateFarmingBatchStatusForTodayJob>("UpdateFarmingBatchStatusForTodayJob", "0 0 1 * * ?", serverTimeZone, cancellationToken);
            await ScheduleJob<Jobs.UpdateTaskStatusesJob>("UpdateTaskStatusesJob-Morning", "0 0 6 * * ?", serverTimeZone, cancellationToken);
            await ScheduleJob<Jobs.UpdateTaskStatusesJob>("UpdateTaskStatusesJob-Noon", "0 0 12 * * ?", serverTimeZone, cancellationToken);
            await ScheduleJob<Jobs.UpdateTaskStatusesJob>("UpdateTaskStatusesJob-Afternoon", "0 0 14 * * ?", serverTimeZone, cancellationToken);
            await ScheduleJob<Jobs.UpdateTaskStatusesJob>("UpdateTaskStatusesJob-Evening", "0 0 18 * * ?", serverTimeZone, cancellationToken);
            await ScheduleJob<Jobs.UpdateTaskStatusesJob>("UpdateTaskStatusesJob-Night", "0 0 23 * * ?", serverTimeZone, cancellationToken);
            //await ScheduleJob<Jobs.UpdateEveningTaskStatusesJob>("UpdateEveningTaskStatusesJob", "0 0 23 * * ?", serverTimeZone, cancellationToken);
            //await ScheduleJob<Jobs.HelloWorldJob>("HelloWorldJob", "*/5 * * * * ?", serverTimeZone, cancellationToken);
            // Job tính toán chi phí hàng ngày, chạy lúc 11h đêm
            await ScheduleJob<Jobs.CalculateDailyCostJob>("CalculateDailyCostJob", "0 0 23 * * ?", serverTimeZone, cancellationToken);
        }

        /// <summary>
        /// Tạo các reminder job và chỉ chạy một lần theo thời gian định sẵn
        /// </summary>
        public async Task CreateReminderJobs(Guid medicalSymptomId, DateTime reportDate)
        {
            var config = await _systemConfigurationService.GetConfigurationAsync();
            var firstReminderTime = DateTimeOffset.Now.AddHours(config.FirstReminderTimeHours); 
            var secondReminderTime = DateTimeOffset.Now.AddHours(config.SecondReminderTimeHours);
            // ✅ Tạo Job lần 1
            var firstReminderJob = JobBuilder.Create<Jobs.MedicalSymptomReminderJob>()
                .WithIdentity($"MedicalSymptomReminderJob-{medicalSymptomId}")
                .UsingJobData("MedicalSymptomId", medicalSymptomId.ToString()) // Truyền dữ liệu vào job
                .Build();

            var firstReminderTrigger = TriggerBuilder.Create()
                .WithIdentity($"MedicalSymptomReminderTrigger-{medicalSymptomId}")
                .StartAt(firstReminderTime) // 🔥 Chạy MỘT LẦN tại thời điểm cụ thể
                .WithSchedule(SimpleScheduleBuilder.Create().WithMisfireHandlingInstructionFireNow()) // Đảm bảo job vẫn chạy nếu bị bỏ lỡ
                .ForJob(firstReminderJob)
                .Build();

            // ✅ Tạo Job lần 2
            var secondReminderJob = JobBuilder.Create<Jobs.MedicalSymptomReminderJob>()
                .WithIdentity($"MedicalSymptomReminderJobLater-{medicalSymptomId}")
                .UsingJobData("MedicalSymptomId", medicalSymptomId.ToString())
                .Build();

            var secondReminderTrigger = TriggerBuilder.Create()
                .WithIdentity($"MedicalSymptomReminderTriggerLater-{medicalSymptomId}")
                .StartAt(secondReminderTime) // 🔥 Chạy MỘT LẦN tại thời điểm cụ thể
                .WithSchedule(SimpleScheduleBuilder.Create().WithMisfireHandlingInstructionFireNow())
                .ForJob(secondReminderJob)
                .Build();



            await _scheduler.ScheduleJob(firstReminderJob, firstReminderTrigger);
            await _scheduler.ScheduleJob(secondReminderJob, secondReminderTrigger);
            Console.WriteLine($"Jobs have been scheduled for MedicalSymptomId: {medicalSymptomId}");
        }

        public async Task<bool> PauseJobAsync(string jobName, CancellationToken cancellationToken)
        {
            var jobKey = new JobKey(jobName);
            if (!await _scheduler.CheckExists(jobKey, cancellationToken)) return false;
            await _scheduler.PauseJob(jobKey, cancellationToken);
            return true;
        }

        public async Task<bool> RemoveJobAsync(string jobName, CancellationToken cancellationToken)
        {
            var jobKey = new JobKey(jobName);
            if (!await _scheduler.CheckExists(jobKey, cancellationToken)) return false;
            var result = await _scheduler.DeleteJob(jobKey, cancellationToken);
            return result;
        }

        public async Task<bool> RescheduleJobAsync(TriggerKey oldTriggerName, ITrigger newTrigger, CancellationToken cancellationToken)
        {
            var result = await _scheduler.RescheduleJob(oldTriggerName, newTrigger, cancellationToken);
            return result != null;
        }

        public async Task<bool> ResumeJobAsync(string jobName, CancellationToken cancellationToken)
        {
            var jobKey = new JobKey(jobName);
            if (!await _scheduler.CheckExists(jobKey, cancellationToken)) return false;
            await _scheduler.ResumeJob(jobKey, cancellationToken);
            return true;
        }

        public async Task<bool> StartJobAsync(string jobName, CancellationToken cancellationToken)
        {
            var jobKey = new JobKey(jobName);
            if (!await _scheduler.CheckExists(jobKey, cancellationToken)) return false;
            await _scheduler.TriggerJob(jobKey, cancellationToken);
            return true;
        }

        public async Task StartSchedulerAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Start(cancellationToken);
        }

        /// <summary>
        /// Hàm helper để tạo job theo cron expression.
        /// </summary>
        private async Task ScheduleJob<T>(string jobName, string cronExpression, TimeZoneInfo timeZone, CancellationToken cancellationToken) where T : IJob
        {
            var jobKey = JobKey.Create(jobName);
            if (await _scheduler.CheckExists(jobKey, cancellationToken))
            {
                await _scheduler.DeleteJob(jobKey, cancellationToken);
            }

            var job = JobBuilder.Create<T>()
                .WithIdentity(jobKey)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{jobName}@Trigger")
                
                .WithCronSchedule(cronExpression, x => { x.InTimeZone(timeZone);
                    //x.WithMisfireHandlingInstructionIgnoreMisfires();
                })
                
                
                .Build();
            Console.WriteLine($"Job {jobName} scheduled successfully.");

            await _scheduler.ScheduleJob(job, trigger, cancellationToken);
        }
        public async Task<bool> CheckSchedulerRunningAsync()
        {
            bool isRunning = _scheduler.IsStarted;
            Console.WriteLine($"Scheduler running status: {isRunning} at {DateTime.Now}");
            await Task.CompletedTask; // Để tương thích với async
            return isRunning;
        }
    }
}
