using Quartz;
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

        public QuartzService(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public async Task LoadBackgroundJobDefault(CancellationToken cancellationToken)
        {
            var serverTimeZone = TimeZoneInfo.Local;

            // Danh sách các jobs mặc định
            await ScheduleJob<Jobs.GenerateTasksForTomorrowJob>("GenerateTasksForTomorrowJob", "0 0 1 * * ?", serverTimeZone, cancellationToken);
            await ScheduleJob<Jobs.UpdateTaskStatusesJob>("UpdateTaskStatusesJob-Morning", "0 0 6 * * ?", serverTimeZone, cancellationToken);
            await ScheduleJob<Jobs.UpdateTaskStatusesJob>("UpdateTaskStatusesJob-Noon", "0 0 12 * * ?", serverTimeZone, cancellationToken);
            await ScheduleJob<Jobs.UpdateTaskStatusesJob>("UpdateTaskStatusesJob-Afternoon", "0 0 14 * * ?", serverTimeZone, cancellationToken);
            await ScheduleJob<Jobs.UpdateTaskStatusesJob>("UpdateTaskStatusesJob-Evening", "0 0 18 * * ?", serverTimeZone, cancellationToken);
            await ScheduleJob<Jobs.UpdateEveningTaskStatusesJob>("UpdateEveningTaskStatusesJob", "0 0 23 * * ?", serverTimeZone, cancellationToken);
            await ScheduleJob<Jobs.HelloWorldJob>("HelloWorldJob", "*/5 * * * * ?", serverTimeZone, cancellationToken);
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
                .WithCronSchedule(cronExpression, x => x.InTimeZone(timeZone))
                .Build();

            await _scheduler.ScheduleJob(job, trigger, cancellationToken);
        }
    }
}
