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
        Task<Guid> CreateDailyFoodUsageLogAsync(DailyFoodUsageLogModel model);
        Task<DailyFoodUsageLogModel> GetDailyFoodUsageLogByIdAsync(Guid id);
    }
}
