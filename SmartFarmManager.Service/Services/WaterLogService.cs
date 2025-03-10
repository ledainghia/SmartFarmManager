using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.ElectricityLog;
using SmartFarmManager.Service.BusinessModels.WaterLog;
using SmartFarmManager.Service.BusinessModels.Webhook;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class WaterLogService : IWaterLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WaterLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<WaterLogInDayModel> GetWaterLogInDayAsync(Guid farmId, DateTime date)
        {
            var waterLog = await _unitOfWork.WaterLogs
                .FindByCondition(w => w.FarmId == farmId && w.CreatedDate.Date == date.Date)
                .FirstOrDefaultAsync();

            if (waterLog == null)
            {
                return null;
            }

            return new WaterLogInDayModel
            {
                FarmId = waterLog.FarmId,
                Data = JsonConvert.DeserializeObject<List<WaterRecordModel>>(waterLog.Data),
                TotalConsumption = waterLog.TotalConsumption
            };
        }
        public async Task<List<WaterLogInDayModel>> GetWaterLogsByDateRangeAsync(Guid farmId, DateTime startDate, DateTime endDate)
        {
            var logs = await _unitOfWork.WaterLogs
                .FindByCondition(e => e.FarmId == farmId && e.CreatedDate.Date >= startDate.Date && e.CreatedDate.Date <= endDate.Date)
                .ToListAsync();

            if (!logs.Any())
            {
                throw new Exception("Không tìm thấy dữ liệu nước trong khoảng thời gian này.");
            }

            // Group theo ngày
            var result = logs
                             .Select(rs => new WaterLogInDayModel
                             {
                                 FarmId = rs.FarmId,
                                 Data = JsonConvert.DeserializeObject<List<WaterRecordModel>>(rs.Data),
                                 TotalConsumption = rs.TotalConsumption
                             }).ToList();

            return result;
        }

        public async Task<WaterLogInMonthModel> GetWaterLogsByMonthAsync(Guid farmId, int month, int year)
        {
            // Lấy tất cả các ElectricityLog trong tháng và năm
            var logs = await _unitOfWork.WaterLogs
           .FindByCondition(e => e.FarmId == farmId && e.CreatedDate.Month == month && e.CreatedDate.Year == year)
           .ToListAsync();

            if (!logs.Any())
            {
                throw new Exception("Không tìm thấy dữ liệu điện trong tháng này.");
            }

            // Tính tổng tiêu thụ trong tháng và trung bình tiêu thụ mỗi ngày
            decimal totalConsumptionInMonth = logs.Sum(e => e.TotalConsumption);
            decimal totalDays = logs.Count;
            decimal averageConsumptionPerDay = totalDays > 0 ? totalConsumptionInMonth / totalDays : 0;

            // Trả về ElectricityLogInMonthModel
            var result = new WaterLogInMonthModel
            {
                Year = year,
                Month = month,
                TotalConsumptionInMonth = totalConsumptionInMonth,
                TotalConsumptionInDateAverage = averageConsumptionPerDay,
                Records = logs.Select(log => new WaterLogInDayModel
                {
                    FarmId = log.FarmId,
                    Data = JsonConvert.DeserializeObject<List<WaterRecordModel>>(log.Data),
                    TotalConsumption = log.TotalConsumption
                }).ToList()
            };

            return result;
        }
        public async Task<List<WaterLogInMonthModel>> GetWaterLogsByYearAsync(Guid farmId, int year)
        {
            var logs = new List<WaterLogInMonthModel>();

            for (int month = 1; month <= 12; month++)
            {
                var electricityLogForMonth = await _unitOfWork.WaterLogs
                    .FindByCondition(e => e.FarmId == farmId && e.CreatedDate.Year == year && e.CreatedDate.Month == month)
                    .ToListAsync();
                if (!electricityLogForMonth.Any())
                {
                    continue;
                }
                decimal totalConsumptionInMonth = electricityLogForMonth.Sum(e => e.TotalConsumption);
                decimal totalDays = electricityLogForMonth.Count;
                decimal averageConsumptionPerDay = totalDays > 0 ? totalConsumptionInMonth / totalDays : 0;

                // Thêm dữ liệu của tháng vào danh sách
                logs.Add(new WaterLogInMonthModel
                {
                    Year = year,
                    Month = month,
                    TotalConsumptionInMonth = totalConsumptionInMonth,
                    TotalConsumptionInDateAverage = averageConsumptionPerDay,
                    Records = electricityLogForMonth.Select(log => new WaterLogInDayModel
                    {
                        FarmId = log.FarmId,
                        Data = JsonConvert.DeserializeObject<List<WaterRecordModel>>(log.Data),
                        TotalConsumption = log.TotalConsumption
                    }).ToList()
                });
            }

            return logs;
        }



    }
}
