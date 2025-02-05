using Quartz;

namespace SmartFarmManager.API.BackgroundJobs.QuartzConfigurations
{
    public class QuartzScheduler
    {
        public static void ConfigureJobs(IServiceCollectionQuartzConfigurator quartzConfig)
        {
            // Lấy múi giờ của server (múi giờ hệ thống)
            var serverTimeZone = TimeZoneInfo.Local;

            // Cấu hình job để chạy đúng với giờ hệ thống của server
            quartzConfig.ScheduleJob<Jobs.GenerateTasksForTomorrowJob>(trigger => trigger
                .WithIdentity("GenerateTasksForTomorrowJob")
                .WithCronSchedule("0 0 1 * * ?", x => x
                    .InTimeZone(serverTimeZone) // Sử dụng giờ của server
                )
            );

            // Job chạy vào đầu các buổi sáng, trưa, chiều theo giờ server
            quartzConfig.ScheduleJob<Jobs.UpdateTaskStatusesJob>(trigger => trigger
                .WithIdentity("UpdateTaskStatusesJob-Morning")
                .WithCronSchedule("0 0 6 * * ?", x => x.InTimeZone(serverTimeZone)) // 6:00 sáng theo giờ server
            );

            quartzConfig.ScheduleJob<Jobs.UpdateTaskStatusesJob>(trigger => trigger
                .WithIdentity("UpdateTaskStatusesJob-Noon")
                .WithCronSchedule("0 0 12 * * ?", x => x.InTimeZone(serverTimeZone)) // 12:00 trưa theo giờ server
            );

            quartzConfig.ScheduleJob<Jobs.UpdateTaskStatusesJob>(trigger => trigger
                .WithIdentity("UpdateTaskStatusesJob-Afternoon")
                .WithCronSchedule("0 0 14 * * ?", x => x.InTimeZone(serverTimeZone)) // 14:00 chiều theo giờ server
            );

            quartzConfig.ScheduleJob<Jobs.UpdateTaskStatusesJob>(trigger => trigger
                .WithIdentity("UpdateTaskStatusesJob-Evening")
                .WithCronSchedule("0 0 18 * * ?", x => x.InTimeZone(serverTimeZone)) // 18:00 tối theo giờ server
            );

            // Job chạy vào lúc 23:59 đêm theo giờ server
            quartzConfig.ScheduleJob<Jobs.UpdateEveningTaskStatusesJob>(trigger => trigger
                .WithIdentity("UpdateEveningTaskStatusesJob")
                .WithCronSchedule("0 0 23 * * ?", x => x.InTimeZone(serverTimeZone)) // 23:59 đêm theo giờ server
            );
        }
    }
}
