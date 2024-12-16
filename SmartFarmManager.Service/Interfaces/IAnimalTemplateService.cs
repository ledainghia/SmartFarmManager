using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.AnimalTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IAnimalTemplateService
    {
        Task<bool> CreateAnimalTemplateAsync(CreateAnimalTemplateModel model);
        Task<bool> UpdateAnimalTemplateAsync(Guid id, UpdateAnimalTemplateModel model);
        Task<bool> ChangeStatusAsync(Guid id, string newStatus);
        Task<bool> DeleteAnimalTemplateAsync(Guid id);
        Task<PagedResult<AnimalTemplateItemModel>> GetFilteredAnimalTemplatesAsync(AnimalTemplateFilterModel filter);
        Task<AnimalTemplateDetailResponseModel> GetAnimalTemplateDetailAsync(Guid id);
    }
}
