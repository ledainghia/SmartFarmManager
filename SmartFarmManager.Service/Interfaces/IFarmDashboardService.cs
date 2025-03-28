using SmartFarmManager.Service.BusinessModels.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IFarmDashboardService
    {
        Task<DashboardStatisticsModel> GetFarmDashboardStatisticsAsync(Guid farmId, DateTime? startDate, DateTime? endDate);
        Task<VaccineScheduleStatisticModel> GetVaccineScheduleStatisticsAsync(Guid farmId, DateTime? startDate, DateTime? endDate);
        Task<StaffStatisticModel> GetStaffStatisticsAsync(Guid farmId);
        Task<FarmingBatchStatisticModel> GetFarmingBatchStatisticsAsync(Guid farmId, DateTime? startDate, DateTime? endDate);
        Task<CageStatisticModel> GetCageStatisticsAsync(Guid farmId);
        Task<TaskStatisticModel> GetTaskStatisticsAsync(Guid farmId, DateTime? startDate, DateTime? endDate);
    }
}
