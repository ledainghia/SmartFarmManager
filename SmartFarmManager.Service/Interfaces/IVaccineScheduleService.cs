using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.VaccineSchedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IVaccineScheduleService
    {
        Task<List<VaccineScheduleResponse>> GetVaccineSchedulesAsync(Guid? stageId, DateTime? date, string status);
        Task<VaccineScheduleResponse> GetVaccineScheduleByIdAsync(Guid id);
        Task<VaccineScheduleWithLogsResponse> GetVaccineScheduleByTaskIdAsync(Guid taskId);
        Task<PagedResult<VaccineScheduleItemModel>> GetVaccineSchedulesAsync(VaccineScheduleFilterKeySearchModel filter);
        Task<bool> CreateVaccineScheduleAsync(CreateVaccineScheduleModel model);
        }
}
