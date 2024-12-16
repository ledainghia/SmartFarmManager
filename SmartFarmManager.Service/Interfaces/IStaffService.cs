using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.Auth;
using SmartFarmManager.Service.BusinessModels.Staff;
using SmartFarmManager.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IStaffService
    {
        Task<List<StaffPendingTasksModel>> GetStaffSortedByPendingTasksAsync(Guid? cageId = null);
        Task<(bool Success, string Message)> AssignStaffToCageAsync(Guid userId, Guid cageId);
        Task<PaginatedList<UserModel>> GetStaffFarmsByFarmIdAsync(Guid farmId, int pageIndex, int pageSize);
    }
}
