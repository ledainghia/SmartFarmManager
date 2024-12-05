using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface ICageService
    {
        Task<PagedResult<CageResponseModel>> GetCagesAsync(CageFilterModel request);
    }
}
