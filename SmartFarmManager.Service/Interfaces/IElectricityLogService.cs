using SmartFarmManager.Service.BusinessModels.ElectricityLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IElectricityLogService
    {
        Task<ElectricityLogInDayModel> GetElectricityLogByDateAsync(Guid farmId, DateTime date);
        Task<ElectricityLogInDayModel> GetElectricityLogForTodayAsync(Guid farmId);
        Task<List<ElectricityLogInDayModel>> GetElectricityLogsByDateRangeAsync(Guid farmId, DateTime startDate, DateTime endDate);
        Task<ElectricityLogInMonthModel> GetElectricityLogsByMonthAsync(Guid farmId, int month, int year);
        Task<List<ElectricityLogInMonthModel>> GetElectricityLogsByYearAsync(Guid farmId, int year);
    }
}
