using SmartFarmManager.Service.BusinessModels.VaccineScheduleLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IVaccineScheduleLogService
    {
        Task<Guid?> CreateVaccineScheduleLogAsync(Guid cageId, VaccineScheduleLogModel model);
        Task<VaccineScheduleLogModel> GetVaccineScheduleLogByIdAsync(Guid id);
    }
}
