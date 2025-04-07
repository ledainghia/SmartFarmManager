using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Sensor;
using SmartFarmManager.Service.BusinessModels.SensorDataLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface ISensorService
    {
        Task<List<SensorDataInDayModel>> GetSensorDataBySensorIdAsync(Guid sensorId, DateTime date);
        Task<List<SensorDataInDayModel>> GetSensorDataBySensorIdRangeAsync(Guid sensorId, DateTime startDate, DateTime endDate);
        Task<List<SensorDataInMonthModel>> GetSensorDataBySensorIdMonthAsync(Guid sensorId, int month, int year);
        Task<List<SensorDataInMonthModel>> GetSensorDataBySensorIdYearAsync(Guid sensorId, int year);
        Task<List<SensorGroupByNodeModel>> GetSensorsByCageIdAsync(Guid cageId);
        Task<PagedResult<SensorItemModel>> GetSensorsAsync(SensorFilterModel filter);
        Task<bool> UpdateSensorAsync(Guid id, UpdateSensorModel model);
        Task<bool> DeleteSensorAsync(Guid id);
    }
}
