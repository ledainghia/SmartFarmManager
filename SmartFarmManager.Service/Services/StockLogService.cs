using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.StockLog;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class StockLogService:IStockLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StockLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddStockAsync(StockLogRequestModel stockLogRequest)
        {
            var farmExist = await _unitOfWork.Farms.FindByCondition(f => f.Id == stockLogRequest.FarmId).FirstOrDefaultAsync();
            if (farmExist == null)
            {
                throw new ArgumentException($"Farm với ID {stockLogRequest.FarmId} không tìm thấy.");
            }

            // Check if FoodType exists in FoodStack
            var foodStack = await _unitOfWork.FoodStacks.FindByCondition(f => f.FoodType == stockLogRequest.FoodType && f.FarmId == farmExist.Id).FirstOrDefaultAsync();
            if (foodStack == null)
            {
                throw new ArgumentException($"FoodStack với FoodType {stockLogRequest.FoodType}không tìm thấy ở Farm {stockLogRequest.FarmId}.");
            }

            var stockLog = new StockLog
            {
                StackId = foodStack.Id,
                FoodType = stockLogRequest.FoodType,
                Quantity = stockLogRequest.Quantity,
                CostPerKg = stockLogRequest.CostPerKg,
                DateAdded = DateOnly.FromDateTime(DateTimeUtils.GetServerTimeInVietnamTime())
            };
            await _unitOfWork.StockLogs.CreateAsync(stockLog);
            await _unitOfWork.CommitAsync();    

            // Update FoodStack and calculate new values
            foodStack.Quantity = stockLogRequest.Quantity+foodStack.CurrentStock;
            foodStack.CurrentStock += stockLogRequest.Quantity;

            var allStockLogs = await _unitOfWork.StockLogs.FindByCondition(sl => sl.StackId == foodStack.Id).ToListAsync();
            var totalCost = allStockLogs.Sum(sl => sl.Quantity * sl.CostPerKg); // Tổng chi phí tất cả các lần nhập
            var totalQuantity = allStockLogs.Sum(sl => sl.Quantity); // Tổng số lượng thức ăn đã nhập

            if (totalQuantity > 0)
            {
                foodStack.CostPerKg = totalCost / totalQuantity; // Tính giá trung bình
            }

            await _unitOfWork.FoodStacks.UpdateAsync(foodStack);

            // Create StockLog record


            await _unitOfWork.CommitAsync();
            return true;
        }
    }
}
