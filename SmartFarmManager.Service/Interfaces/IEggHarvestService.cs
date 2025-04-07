using SmartFarmManager.Service.BusinessModels.EggHarvest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IEggHarvestService
    {
        Task<bool> CreateEggHarvestAsync(CreateEggHarvestRequest request);
        Task<IEnumerable<EggHarvestResponse>> GetEggHarvestsByTaskIdAsync(Guid taskId);
    }
}
