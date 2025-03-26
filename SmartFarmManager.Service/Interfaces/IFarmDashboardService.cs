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
    }
}
