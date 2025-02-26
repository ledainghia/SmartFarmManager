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
    }
}
