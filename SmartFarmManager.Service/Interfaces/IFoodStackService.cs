using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.FoodStack;
using SmartFarmManager.Service.BusinessModels.StockLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IFoodStackService
    {
        Task<bool> CreateFoodStackAsync(FoodStackCreateModel model);
        Task<PagedResult<FoodStackItemModel>> GetFoodStacksAsync(FoodStackFilterModel filter);
        Task<PagedResult<StockLogItemModel>> GetStockLogHistoryAsync(Guid foodStackId, int  pageNumber, int pageSize);
        Task<bool> UpdateFoodStackAsync(Guid id, UpdateFoodStockModel request);
        Task<bool> DeleteFoodStackAsync(Guid id);
    }
}
