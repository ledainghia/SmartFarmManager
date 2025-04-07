using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Sensor;
using SmartFarmManager.Service.BusinessModels.SensorDataLog;
using SmartFarmManager.Service.Helpers;
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


        public async Task<PagedResult<SensorItemModel>> GetSensorsAsync(SensorFilterModel filter)
        {
            var query = _unitOfWork.Sensors
                .FindByCondition(s => s.Cage.FarmId == filter.FarmId)
                .Include(s => s.SensorType)
                .Include(s => s.Cage)
                .AsQueryable();
            if (!string.IsNullOrEmpty(filter.KeySearch))
            {
                query = query.Where(s => s.SensorCode.Contains(filter.KeySearch) || s.Name.Contains(filter.KeySearch)||
                s.Cage.Name.Contains(filter.KeySearch)||
                s.SensorType.Name.Contains(filter.KeySearch));
            }
            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(s => s.Status.ToString() == filter.Status);
            }
            if (filter.SensorTypeId.HasValue)
            {
                query = query.Where(s => s.SensorTypeId == filter.SensorTypeId.Value);
            }

            if (filter.NodeId.HasValue)
            {
                query = query.Where(s => s.NodeId == filter.NodeId.Value);
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(s => new SensorItemModel
                {
                    Id = s.Id,
                    SensorTypeId = s.SensorTypeId,
                    SensorTypeName = s.SensorType.Name,
                    CageId = s.CageId,
                    CageName = s.Cage.Name,
                    NodeId = s.NodeId,
                    PinCode = s.PinCode,
                    IsDeleted = s.IsDeleted,
                    DeletedDate = s.DeletedDate,
                    SensorCode = s.SensorCode,
                    Name = s.Name,
                    Status = s.Status
                })
                .ToListAsync();

            var result = new PaginatedList<SensorItemModel>(items, totalItems, filter.PageNumber, filter.PageSize);
            return new PagedResult<SensorItemModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage
            };
        }

        public async Task<bool> UpdateSensorAsync(Guid id, UpdateSensorModel model)
        {
            var existingSensor = await _unitOfWork.Sensors
                .FindByCondition(s => s.Id == id)
                .FirstOrDefaultAsync();
            if (existingSensor == null)
            {
                throw new KeyNotFoundException($"Sensor với  ID {id} không tìm thấy .");
            }

            if (model.CageId.HasValue)
            {
                var existingCage = await _unitOfWork.Cages
                    .FindByCondition(c => c.Id == model.CageId)
                    .FirstOrDefaultAsync();

                if (existingCage == null)
                {
                    throw new KeyNotFoundException($"Chuồng với ID {model.CageId} không tìm thấy .");

                }
            }
            if (model.SensorTypeId.HasValue)
            {
                var existingSensorType = await _unitOfWork.SensorTypes
                    .FindByCondition(st => st.Id == model.SensorTypeId)
                    .FirstOrDefaultAsync();
                if (existingSensorType == null)
                {
                    throw new KeyNotFoundException($"Loại cảm biến với {model.SensorTypeId} không tìm thấy.");
                }
            }
            existingSensor.SensorTypeId = model.SensorTypeId ?? existingSensor.SensorTypeId;
            existingSensor.CageId = model.CageId ?? existingSensor.CageId;
            existingSensor.Name = model.Name ?? existingSensor.Name;
            existingSensor.PinCode = model.PinCode ??existingSensor.PinCode;
            existingSensor.Status = model.Status ?? existingSensor.Status;
            existingSensor.NodeId = model.NodeId ?? existingSensor.NodeId;

            await _unitOfWork.Sensors.UpdateAsync(existingSensor);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<bool> DeleteSensorAsync(Guid id)
        {
            var existingSensor = await _unitOfWork.Sensors
                .FindByCondition(s => s.Id == id)
                .FirstOrDefaultAsync();

            if (existingSensor == null)
            {
                throw new KeyNotFoundException($"Cảm biến với Id {id} không tìm thấy!.");
            }
            existingSensor.IsDeleted = true;
            existingSensor.DeletedDate =DateTimeUtils.GetServerTimeInVietnamTime(); 

            await _unitOfWork.Sensors.UpdateAsync(existingSensor);
            await _unitOfWork.CommitAsync();

            return true;
        }
    }
}
