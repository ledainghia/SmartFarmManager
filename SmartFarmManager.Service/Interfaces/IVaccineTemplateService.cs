using SmartFarmManager.Service.BusinessModels.AnimalTemplate;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.VaccineTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IVaccineTemplateService
    {
        Task<bool> CreateVaccineTemplateAsync(CreateVaccineTemplateModel model);
        Task<bool> UpdateVaccineTemplateAsync(UpdateVaccineTemplateModel model);
        Task<bool> DeleteVaccineTemplateAsync(Guid id);
        Task<PagedResult<VaccineTemplateItemModel>> GetVaccineTemplatesAsync(VaccineTemplateFilterModel filter);
        Task<VaccineTemplateDetailModel?> GetVaccineTemplateDetailAsync(Guid id);
    }
}
