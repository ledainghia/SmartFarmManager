using SmartFarmManager.Service.BusinessModels.AnimalTemplate;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.GrowthStageTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IGrowthStageTemplateService
    {
        Task<bool> CreateGrowthStageTemplateAsync(CreateGrowthStageTemplateModel model);
        Task<bool> UpdateGrowthStageTemplateAsync(Guid id ,UpdateGrowthStageTemplateModel model);
        Task<bool> DeleteGrowthStageTemplateAsync(Guid id);
        Task<PagedResult<GrowthStageTemplateItemModel>> GetGrowthStageTemplatesAsync(GrowthStageTemplateFilterModel filter);
        Task<GrowthStageTemplateDetailResponseModel?> GetGrowthStageTemplateDetailAsync(Guid id);
    }
}
    