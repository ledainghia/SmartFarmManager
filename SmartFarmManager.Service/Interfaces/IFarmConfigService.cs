using SmartFarmManager.Service.BusinessModels.FarmConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IFarmConfigService
    {
        Task UpdateFarmTimeDifferenceAsync(Guid farmId, DateTime newTime);
        Task ResetTimeDifferenceAsync(Guid farmId);
        Task<bool> UpdateFarmConfigAsync(Guid farmId, FarmConfigUpdateModel model);
    }
}
