using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.FarmingBatch;
using SmartFarmManager.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IFarmingBatchService
    {
        Task<bool> CreateFarmingBatchAsync(CreateFarmingBatchModel model);
        Task<bool> UpdateFarmingBatchStatusAsync(Guid farmingBatchId, string newStatus);
        Task<PagedResult<FarmingBatchModel>> GetFarmingBatchesAsync(string? cageName, string? name, string? species, DateTime? startDateFrom, DateTime? startDateTo, int pageNumber, int pageSize, Guid? cageId, bool? isCancel);
        Task<FarmingBatchModel> GetActiveFarmingBatchByCageIdAsync(Guid cageId);
        Task<List<FarmingBatchModel>> GetActiveFarmingBatchesByUserAsync(Guid userId);
        Task<FarmingBatchReportResponse> GetFarmingBatchReportAsync(Guid farmingBatchId);
        Task<DetailedFarmingBatchReportResponse> GetDetailedFarmingBatchReportAsync(Guid farmingBatchId);
    }
}
