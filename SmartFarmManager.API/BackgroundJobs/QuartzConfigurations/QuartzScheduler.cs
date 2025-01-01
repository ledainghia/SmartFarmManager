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
        }
    }
}
