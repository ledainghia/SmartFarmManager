using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Disease;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IDiseaseService
    {
        Task<PagedResult<DiseaseModel>> GetPagedDiseasesAsync(string? name, int page, int pageSize);
        Task<bool> CreateDiseaseAsync(CreateDiseaseModel model);
        Task<bool> UpdateDiseaseAsync(Guid id, UpdateDiseaseModel model);
        Task<PagedResult<DiseaseItemModel>> GetDiseasesAsync(DiseaseFilterModel filter);
        Task<bool> DeleteDiseaseAsync(Guid id);
        Task<DiseaseDetailResponseModel> GetDiseaseDetailAsync(Guid id);
    }
}
