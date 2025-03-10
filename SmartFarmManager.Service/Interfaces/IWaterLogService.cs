using SmartFarmManager.Service.BusinessModels.WaterLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IWaterLogService
    {
        Task<WaterLogInDayModel> GetWaterLogInDayAsync(Guid farmId, DateTime date);
        Task<List<WaterLogInDayModel>> GetWaterLogsByDateRangeAsync(Guid farmId, DateTime startDate, DateTime endDate);
        Task<WaterLogInMonthModel> GetWaterLogsByMonthAsync(Guid farmId, int month, int year);
        Task<List<WaterLogInMonthModel>> GetWaterLogsByYearAsync(Guid farmId, int year);
    }
}
