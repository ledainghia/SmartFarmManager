using SmartFarmManager.Service.BusinessModels.DailyFoodUsageLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IDailyFoodUsageLogService
    {
        Task<DailyFoodUsageLogModel> GetDailyFoodUsageLogByIdAsync(Guid id);
        Task<Guid?> CreateDailyFoodUsageLogAsync(Guid cageId, DailyFoodUsageLogModel model);
        Task<DailyFoodUsageLogModel> GetDailyFoodUsageLogByTaskIdAsync(Guid taskId);
    }
}
