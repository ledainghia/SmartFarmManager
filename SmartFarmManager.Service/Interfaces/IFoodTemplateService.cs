using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.FoodTemplate;
using SmartFarmManager.Service.BusinessModels.GrowthStageTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IFoodTemplateService
    {
        Task<bool> CreateFoodTemplateAsync(CreateFoodTemplateModel model);
        Task<bool> UpdateFoodTemplateAsync(UpdateFoodTemplateModel model);
        Task<bool> DeleteFoodTemplateAsync(Guid id);
        Task<PagedResult<FoodTemplateItemModel>> GetFoodTemplatesAsync(FoodTemplateFilterModel filter);
        Task<FoodTemplateDetailModel?> GetFoodTemplateDetailAsync(Guid id);

    }
}
