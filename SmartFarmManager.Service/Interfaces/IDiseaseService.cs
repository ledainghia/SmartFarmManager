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
    }
}
