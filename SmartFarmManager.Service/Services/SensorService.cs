using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Sensor;
using SmartFarmManager.Service.BusinessModels.SensorDataLog;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class SensorService:ISensorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SensorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<SensorGroupByNodeModel>> GetSensorsByCageIdAsync(Guid cageId)
        {
            // Truy vấn các sensor theo CageId
            var sensors = await _unitOfWork.Sensors
                .FindByCondition(s => s.CageId == cageId && !s.IsDeleted)
                .Include(s => s.SensorType)
                .ToListAsync();

            if (!sensors.Any())
            {
                throw new Exception("No sensors found for this cage.");
            }

            // Nhóm sensor theo NodeId
            var groupedSensors = sensors
                .GroupBy(s => s.NodeId)
                .Select(g => new SensorGroupByNodeModel
                {
                    NodeId = g.Key,
                    Sensors = g.Select(s => new SensorModel
                    {
                        SensorId = s.Id,
                        SensorCode = s.SensorCode,
                        Name = s.Name,
                        SensorTypeName = s.SensorType.Name,
                        PinCode = s.PinCode,
                        Status = s.Status
                    }).ToList()
                }).ToList();

            return groupedSensors;
        }
        public async Task<List<SensorDataInDayModel>> GetSensorDataBySensorIdAsync(Guid sensorId, DateTime date)
        {
            var logs = await _unitOfWork.SensorDataLogs
                .FindByCondition(e => e.SensorId == sensorId && e.CreatedDate.Date == date.Date)
                .ToListAsync();

            if (!logs.Any())
            {
                throw new Exception("No sensor data found for this sensor and date.");
            }

            return logs.Select(log => new SensorDataInDayModel
            {
                SensorId = log.SensorId,
                IsWarning = log.IsWarning,
                Data = JsonConvert.DeserializeObject<List<SensorRecordModel>>(log.Data)
            }).ToList();
        }

        public async Task<List<SensorDataInDayModel>> GetSensorDataBySensorIdRangeAsync(Guid sensorId, DateTime startDate, DateTime endDate)
        {
            var logs = await _unitOfWork.SensorDataLogs
                .FindByCondition(e => e.SensorId == sensorId && e.CreatedDate.Date >= startDate.Date && e.CreatedDate.Date <= endDate.Date)
                .ToListAsync();

            if (!logs.Any())
            {
                throw new Exception("No sensor data found in this date range.");
            }

            return logs.Select(log => new SensorDataInDayModel
            {
                SensorId = log.SensorId,
                IsWarning = log.IsWarning,
                Data = JsonConvert.DeserializeObject<List<SensorRecordModel>>(log.Data)
            }).ToList();
        }
        public async Task<List<SensorDataInMonthModel>> GetSensorDataBySensorIdMonthAsync(Guid sensorId, int month, int year)
        {
            var logs = await _unitOfWork.SensorDataLogs
                .FindByCondition(e => e.SensorId == sensorId && e.CreatedDate.Month == month && e.CreatedDate.Year == year)
                .ToListAsync();

            if (!logs.Any())
            {
                throw new Exception("No sensor data found for this month.");
            }

            var groupedLogs = logs
                .GroupBy(log => new { log.CreatedDate.Day })
                .Select(g => new SensorDataInDayModel
                {
                    SensorId = g.First().SensorId,
                    IsWarning = g.First().IsWarning,
                    Data = JsonConvert.DeserializeObject<List<SensorRecordModel>>(g.First().Data)
                }).ToList();

            return new List<SensorDataInMonthModel>
        {
            new SensorDataInMonthModel
            {
                Year = year,
                Month = month,
                Records = groupedLogs
            }
        };
        }
        public async Task<List<SensorDataInMonthModel>> GetSensorDataBySensorIdYearAsync(Guid sensorId, int year)
        {
            var logs = await _unitOfWork.SensorDataLogs
                .FindByCondition(e => e.SensorId == sensorId && e.CreatedDate.Year == year)
                .ToListAsync();

            if (!logs.Any())
            {
                throw new Exception("No sensor data found for this year.");
            }

            var groupedLogs = logs
                .GroupBy(log => new { log.CreatedDate.Month })
                .Select(g => new SensorDataInMonthModel
                {
                    Year = year,
                    Month = g.Key.Month,
                    Records = g.Select(log => new SensorDataInDayModel
                    {
                        SensorId = log.SensorId,
                        IsWarning = log.IsWarning,
                        Data = JsonConvert.DeserializeObject<List<SensorRecordModel>>(log.Data)
                    }).ToList()
                }).ToList();

            return groupedLogs;
        }
    }
}
