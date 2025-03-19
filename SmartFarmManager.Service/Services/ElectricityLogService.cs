using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.ElectricityLog;
using SmartFarmManager.Service.BusinessModels.Webhook;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class ElectricityLogService: IElectricityLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ElectricityLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ElectricityLogInDayModel> GetElectricityLogByDateAsync(Guid farmId,DateTime date)
        {
            var log = await _unitOfWork.ElectricityLogs
            .FindByCondition(e => e.FarmId == farmId && e.CreatedDate.Date == date.Date)
            .FirstOrDefaultAsync();

            if(log == null)
            {
               throw new Exception("Không tìm thấy dữ liệu điện cho ngày này.");
            }

            var data = JsonConvert.DeserializeObject<List<ElectricRecordModel>>(log.Data);
            
            return new ElectricityLogInDayModel
            {
                FarmId = log.FarmId,
                Data = data,
                TotalConsumption = log.TotalConsumption
            };
        }
        public async Task<ElectricityLogInDayModel> GetElectricityLogForTodayAsync(Guid farmId)
        {
            var today = DateTimeUtils.GetServerTimeInVietnamTime().Date;
            return await GetElectricityLogByDateAsync(farmId, today);
        }

        public async Task<List<ElectricityLogInDayModel>> GetElectricityLogsByDateRangeAsync(Guid farmId, DateTime startDate, DateTime endDate)
        {
            var logs = await _unitOfWork.ElectricityLogs
                .FindByCondition(e => e.FarmId == farmId && e.CreatedDate.Date >= startDate.Date && e.CreatedDate.Date <= endDate.Date)
                .ToListAsync();

            if (!logs.Any())
            {
                throw new Exception("Không tìm thấy dữ liệu điện trong khoảng thời gian này.");
            }

            // Group theo ngày
            var result = logs
                             .Select(rs => new ElectricityLogInDayModel
                             {
                                 FarmId = rs.FarmId,
                                 Data = JsonConvert.DeserializeObject<List<ElectricRecordModel>>(rs.Data),
                                 TotalConsumption = rs.TotalConsumption
                             }).ToList();

            return result;
        }

        public async Task<ElectricityLogInMonthModel> GetElectricityLogsByMonthAsync(Guid farmId, int month, int year)
        {
            // Lấy tất cả các ElectricityLog trong tháng và năm
            var logs = await _unitOfWork.ElectricityLogs
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
            var result = new ElectricityLogInMonthModel
            {
                Year = year,
                Month = month,
                TotalConsumptionInMonth = totalConsumptionInMonth,
                TotalConsumptionInDateAverage = averageConsumptionPerDay,
                Records = logs.Select(log => new ElectricityLogInDayModel
                {
                    FarmId = log.FarmId,
                    Data = JsonConvert.DeserializeObject<List<ElectricRecordModel>>(log.Data),
                    TotalConsumption = log.TotalConsumption
                }).ToList()
            };

            return result;
        }

        public async Task<List<ElectricityLogInMonthModel>> GetElectricityLogsByYearAsync(Guid farmId, int year)
        {
            var logs = new List<ElectricityLogInMonthModel>();

            for (int month = 1; month <= 12; month++)
            {
                var electricityLogForMonth = await _unitOfWork.ElectricityLogs
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
                logs.Add(new ElectricityLogInMonthModel
                {
                    Year = year,
                    Month = month,
                    TotalConsumptionInMonth = totalConsumptionInMonth,
                    TotalConsumptionInDateAverage = averageConsumptionPerDay,
                    Records = electricityLogForMonth.Select(log => new ElectricityLogInDayModel
                    {
                        FarmId = log.FarmId,
                        Data = JsonConvert.DeserializeObject<List<ElectricRecordModel>>(log.Data),
                        TotalConsumption = log.TotalConsumption
                    }).ToList()
                });
            } 

            return logs;
        }

    }
}   
