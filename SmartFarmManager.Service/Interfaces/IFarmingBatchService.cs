using SmartFarmManager.Service.BusinessModels.FarmingBatch;
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
<<<<<<< Updated upstream
=======
        Task<PagedResult<FarmingBatchModel>> GetFarmingBatchesAsync(string? status, string? cageName, string? name, string? species, DateTime? startDateFrom, DateTime? startDateTo, int page, int pageSize, Guid? cageId);
>>>>>>> Stashed changes
    }
}
