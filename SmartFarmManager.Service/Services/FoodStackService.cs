using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.FoodStack;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class FoodStackService : IFoodStackService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FoodStackService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateFoodStackAsync(FoodStackCreateModel model)
        {
            // Kiểm tra xem Farm có tồn tại không
            var farmExist = await _unitOfWork.Farms.FindByCondition(f => f.Id == model.FarmId).FirstOrDefaultAsync();
            if (farmExist == null)
            {
                throw new KeyNotFoundException($"Farm with Id: {model.FarmId} not found!");
            }

            // Kiểm tra xem FoodStack có tồn tại trong Farm này chưa
            var foodStackExist = await _unitOfWork.FoodStacks.FindByCondition(f => f.FoodType == model.FoodType && f.FarmId == model.FarmId).FirstOrDefaultAsync();
            if (foodStackExist != null)
            {
                throw new InvalidOperationException($"Food Stack with Food Type: {model.FoodType} already exists in this farm!");
            }

            // Tạo mới FoodStack
            var newFoodStack = new FoodStack
            {
                FarmId = model.FarmId,
                FoodType = model.FoodType,
                CostPerKg = model.CostPerKg,
                Quantity = model.Quantity,
                CurrentStock = model.Quantity
            };
            var foodStackId = await _unitOfWork.FoodStacks.CreateAsync(newFoodStack);
            await _unitOfWork.CommitAsync();

        
            if (foodStackId == null)
            {
                throw new Exception("Error while creating Food Stack!");
            }

            // Tạo StockLog
            var stockLog = new StockLog
            {
                StackId = foodStackId,
                FoodType = model.FoodType,
                Quantity = model.Quantity,
                CostPerKg = model.CostPerKg,
                DateAdded = DateOnly.FromDateTime(DateTimeUtils.GetServerTimeInVietnamTime())
            };

            // Lưu StockLog và commit các thay đổi trong một lần duy nhất
            await _unitOfWork.StockLogs.CreateAsync(stockLog);
            await _unitOfWork.CommitAsync();  // Gọi commit một lần duy nhất cho tất cả các thao tác

            return true;


        }
    }
}
