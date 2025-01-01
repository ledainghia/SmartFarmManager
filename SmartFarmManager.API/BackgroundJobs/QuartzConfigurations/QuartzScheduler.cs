using Quartz;

namespace SmartFarmManager.API.BackgroundJobs.QuartzConfigurations
{
    public class QuartzScheduler
    {
        public static void ConfigureJobs(IServiceCollectionQuartzConfigurator quartzConfig)
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

           // quartzConfig.ScheduleJob<Jobs.HelloWorldJob>(trigger => trigger
           //    .WithIdentity("HelloWorldJob")
           //    .WithCronSchedule("0 5 18 * * ?", x => x
           //        .InTimeZone(vietnamTimeZone) // Áp dụng múi giờ Việt Nam
           //    )
           //);
            quartzConfig.ScheduleJob<Jobs.GenerateTasksForTomorrowJob>(trigger => trigger
                .WithIdentity("GenerateTasksForTomorrowJob")
                .WithCronSchedule("0 0 1 * * ?", x => x
                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")) // Múi giờ Việt Nam
                )
            );
            // UpdateTaskStatusesJob chạy vào cuối các buổi
            quartzConfig.ScheduleJob<Jobs.UpdateTaskStatusesJob>(trigger => trigger
                .WithIdentity("UpdateTaskStatusesJob-Morning")
                .WithCronSchedule("0 59 11 * * ?", x => x.InTimeZone(vietnamTimeZone)) // 11:59 sáng
            );

            quartzConfig.ScheduleJob<Jobs.UpdateTaskStatusesJob>(trigger => trigger
                .WithIdentity("UpdateTaskStatusesJob-Afternoon")
                .WithCronSchedule("0 59 17 * * ?", x => x.InTimeZone(vietnamTimeZone)) // 17:59 chiều
            );

            quartzConfig.ScheduleJob<Jobs.UpdateTaskStatusesJob>(trigger => trigger
                .WithIdentity("UpdateTaskStatusesJob-Evening")
                .WithCronSchedule("0 59 23 * * ?", x => x.InTimeZone(vietnamTimeZone)) // 23:59 tối
            );
        }
    }
}
