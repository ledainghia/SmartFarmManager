using SmartFarmManager.Service.BusinessModels.GrowthStage;
using SmartFarmManager.Service.BusinessModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFarmManager.Service.BusinessModels.TaskDaily;
using SmartFarmManager.Service.BusinessModels.VaccineSchedule;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IGrowthStageService
    {
        Task<PagedResult<GrowthStageItemModel>> GetGrowthStagesAsync(GrowthStageFilterModel filter);
        Task<GrowthStageDetailModel> GetGrowthStageDetailAsync(Guid id);
        Task<PagedResult<TaskDailyModel>> GetTaskDailiesByGrowthStageIdAsync(TaskDailyFilterModel filter);
        Task<PagedResult<VaccineScheduleModel>> GetVaccineSchedulesByGrowthStageIdAsync(VaccineScheduleFilterModel filter);
        Task<GrowthStageDetailModel> GetActiveGrowthStageByCageIdAsync(Guid cageId);
    }
      
}
