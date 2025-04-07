using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class CostingService : ICostingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CostingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async System.Threading.Tasks.Task CalculateAndStoreDailyCostAsync()
        {
            var today = DateTimeUtils.GetServerTimeInVietnamTime().Date;
            var reportMonth = today.Month;
            var reportYear = today.Year;

            var farms = await _unitOfWork.Farms.FindAll().ToListAsync();

            foreach (var farm in farms)
            {
                var existingReport = await _unitOfWork.CostingReports
                    .FindByCondition(r => r.FarmId == farm.Id && r.ReportMonth == reportMonth && r.ReportYear == reportYear)
                    .ToListAsync();

                var costData = await GetDailyCostsForFarm(farm.Id, today);

                foreach (var cost in costData)
                {
                    var existingCostEntry = existingReport.FirstOrDefault(r => r.CostType == cost.CostType);

                    if (existingCostEntry != null)
                    {
                        // Cập nhật báo cáo
                        existingCostEntry.TotalQuantity += cost.TotalQuantity;
                        existingCostEntry.TotalCost += cost.TotalCost;
                        existingCostEntry.GeneratedAt = today;

                        await _unitOfWork.CostingReports.UpdateAsync(existingCostEntry);
                    }
                    else
                    {
                        // Tạo báo cáo mới
                        var newReport = new CostingReport
                        {
                            Id = Guid.NewGuid(),
                            FarmId = farm.Id,
                            ReportMonth = reportMonth,
                            ReportYear = reportYear,
                            CostType = cost.CostType,
                            TotalQuantity = cost.TotalQuantity,
                            TotalCost = cost.TotalCost,
                            GeneratedAt = today
                        };

                        await _unitOfWork.CostingReports.CreateAsync(newReport);
                    }
                }
            }

            await _unitOfWork.CommitAsync();
        }

        private async Task<List<CostingReport>> GetDailyCostsForFarm(Guid farmId, DateTime date)
        {
            var costReports = new List<CostingReport>();

            // 1️⃣ Chi phí điện
            var electricityLogs = await _unitOfWork.ElectricityLogs
                .FindByCondition(e => e.FarmId == farmId && e.CreatedDate.Date == date)
                .ToListAsync();

            var electricityUsage = electricityLogs.Sum(e => e.TotalConsumption);
            var electricityPrice = await _unitOfWork.MasterData
                .FindByCondition(m => m.FarmId == farmId && m.CostType == "Điện")
                .Select(m => m.UnitPrice)
                .FirstOrDefaultAsync();

            costReports.Add(new CostingReport
            {
                CostType = "Điện",
                TotalQuantity = (decimal)electricityUsage,
                TotalCost = (decimal)electricityUsage * electricityPrice
            });

            // 2️⃣ Chi phí nước
            var waterLogs = await _unitOfWork.WaterLogs
                .FindByCondition(w => w.FarmId == farmId && w.CreatedDate.Date == date)
                .ToListAsync();

            var waterUsage = waterLogs.Sum(w => w.TotalConsumption);
            var waterPrice = await _unitOfWork.MasterData
                .FindByCondition(m => m.FarmId == farmId && m.CostType == "Nước")
                .Select(m => m.UnitPrice)
                .FirstOrDefaultAsync();

            costReports.Add(new CostingReport
            {
                CostType = "Nước",
                TotalQuantity = (decimal)waterUsage,
                TotalCost = (decimal)waterUsage * waterPrice
            });

            // 3️⃣ Chi phí thức ăn
            var foodLogs = await _unitOfWork.DailyFoodUsageLogs
                .FindByCondition(f => f.Stage.FarmingBatch.FarmId == farmId && f.LogTime.Value.Date == date)
                .ToListAsync();

            var totalFoodCost = foodLogs.Sum(f => f.ActualWeight.Value * (decimal)f.UnitPrice);

            costReports.Add(new CostingReport
            {
                CostType = "Thức ăn",
                TotalQuantity = foodLogs.Sum(f => f.ActualWeight ?? 0),
                TotalCost = totalFoodCost
            });

            // 4️⃣ Chi phí vaccine
            var vaccineLogs = await _unitOfWork.VaccineScheduleLogs
                .FindByCondition(v => v.Schedule.Stage.FarmingBatch.FarmId == farmId && v.Date.Value == DateOnly.FromDateTime(date))
                .ToListAsync();

            var totalVaccineCost = vaccineLogs.Sum(v => v.Schedule.ToltalPrice);

            costReports.Add(new CostingReport
            {
                CostType = "Vaccine",
                TotalQuantity = vaccineLogs.Count(),
                TotalCost = totalVaccineCost.Value
            });

            // 5️⃣ Chi phí thuốc
            var prescriptionLogs = await _unitOfWork.Prescription
                .FindByCondition(p => p.MedicalSymtom.FarmingBatch.FarmId == farmId && p.PrescribedDate.Value.Date == date)
                .ToListAsync();

            var totalMedicineCost = prescriptionLogs.Sum(p => p.Price ?? 0);

            costReports.Add(new CostingReport
            {
                CostType = "Thuốc",
                TotalQuantity = prescriptionLogs.Count,
                TotalCost = totalMedicineCost
            });

            // 6️⃣ Doanh thu bán thịt
            var meatSales = await _unitOfWork.AnimalSales
                .FindByCondition(s => s.FarmingBatch.FarmId == farmId && s.SaleDate.Value.Date == date && s.SaleType.StageTypeName == "MeatSale")
                .ToListAsync();

            var totalMeatRevenue = meatSales.Sum(s => s.UnitPrice * s.Quantity);

            costReports.Add(new CostingReport
            {
                CostType = "Doanh thu bán thịt",
                TotalQuantity = meatSales.Sum(s => s.Quantity),
                TotalCost = (decimal)totalMeatRevenue
            });

            // 7️⃣ Doanh thu bán trứng
            var eggSales = await _unitOfWork.AnimalSales
                .FindByCondition(s => s.FarmingBatch.FarmId == farmId && s.SaleDate.Value.Date == date && s.SaleType.StageTypeName == "EggSale")
                .ToListAsync();

            var totalEggRevenue = eggSales.Sum(s => s.UnitPrice * s.Quantity);

            costReports.Add(new CostingReport
            {
                CostType = "Doanh thu bán trứng",
                TotalQuantity = eggSales.Sum(s => s.Quantity),
                TotalCost = (decimal)totalEggRevenue
            });

            return costReports;
        }
    }

}
