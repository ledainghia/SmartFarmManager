using Microsoft.EntityFrameworkCore.Metadata;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IQuartzService

    {
        Task LoadBackgroundJobDefault(CancellationToken cancellationToken);
        Task<bool> StartJobAsync(string jobName, CancellationToken cancellationToken);
        Task<bool> RemoveJobAsync(string jobName, CancellationToken cancellationToken);
        Task<bool> PauseJobAsync(string jobName, CancellationToken cancellationToken);
        Task<bool> ResumeJobAsync(string jobName, CancellationToken cancellationToken);

        Task StartSchedulerAsync(CancellationToken cancellationToken);

        Task<bool> RescheduleJobAsync(TriggerKey oldTriggerName, Quartz.ITrigger newTrigger, CancellationToken cancellationToken);
    }
}
