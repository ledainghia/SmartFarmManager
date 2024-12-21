using SmartFarmManager.Service.BusinessModels.HealthLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IHealthLogService
    {
        Task<Guid> CreateHealthLogAsync(HealthLogModel model);
        Task<HealthLogModel> GetHealthLogByIdAsync(Guid id);
        Task<IEnumerable<HealthLogModel>> GetHealthLogsAsync(Guid? prescriptionId);
    }
}
