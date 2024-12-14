using SmartFarmManager.Service.BusinessModels.Farm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IFarmService
    {
        
            Task<Guid> CreateFarmAsync(FarmModel model);
            Task<FarmModel> GetFarmByIdAsync(Guid id);
            Task<IEnumerable<FarmModel>> GetAllFarmsAsync(string? search);
            Task<bool> UpdateFarmAsync(Guid id, FarmModel model);
            Task<bool> DeleteFarmAsync(Guid id);

    }
}
