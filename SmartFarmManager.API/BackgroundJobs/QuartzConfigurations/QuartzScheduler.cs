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
                    .InTimeZone(vietnamTimeZone) // Múi giờ Việt Nam
                )
            );
            // Job chạy vào đầu các buổi
            quartzConfig.ScheduleJob<Jobs.UpdateTaskStatusesJob>(trigger => trigger
     .WithIdentity("UpdateTaskStatusesJob-Morning")
     .WithCronSchedule("0 0 6 * * ?", x => x.InTimeZone(vietnamTimeZone)) // 6:00 sáng
 );

            quartzConfig.ScheduleJob<Jobs.UpdateTaskStatusesJob>(trigger => trigger
                .WithIdentity("UpdateTaskStatusesJob-Noon")
                .WithCronSchedule("0 0 12 * * ?", x => x.InTimeZone(vietnamTimeZone)) // 12:00 trưa
            );

            quartzConfig.ScheduleJob<Jobs.UpdateTaskStatusesJob>(trigger => trigger
                .WithIdentity("UpdateTaskStatusesJob-Afternoon")
                .WithCronSchedule("0 0 14 * * ?", x => x.InTimeZone(vietnamTimeZone)) // 14:00 chiều
            );

            quartzConfig.ScheduleJob<Jobs.UpdateTaskStatusesJob>(trigger => trigger
                .WithIdentity("UpdateTaskStatusesJob-Evening")
                .WithCronSchedule("0 0 18 * * ?", x => x.InTimeZone(vietnamTimeZone)) // 18:00 tối
            );


            quartzConfig.ScheduleJob<Jobs.UpdateEveningTaskStatusesJob>(trigger => trigger
                .WithIdentity("UpdateEveningTaskStatusesJob")
                .WithCronSchedule("0 59 23 * * ?", x => x.InTimeZone(vietnamTimeZone)) // 23:59 đêm
            );
        }
    }
}
