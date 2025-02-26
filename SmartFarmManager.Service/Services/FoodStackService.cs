using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.FoodStack;
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

        public async Task<PagedResult<FoodStackItemModel>> GetFoodStacksAsync(FoodStackFilterModel filter)
        {
            var query = _unitOfWork.FoodStacks.FindAll(false).AsQueryable();

            if (filter.FarmId.HasValue)
            {
                query = query.Where(f => f.FarmId == filter.FarmId.Value);
            }

            if (!string.IsNullOrEmpty(filter.FoodType))
            {
                query = query.Where(f => f.FoodType.Contains(filter.FoodType));
            }
            var totalItems = await query.CountAsync();

            // Paginate and fetch the data
            var items = await query
                .OrderBy(f => f.FoodType)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(f => new FoodStackItemModel
                {
                    Id = f.Id,
                    FarmId = f.FarmId,
                    FoodType = f.FoodType,
                    Quantity = (decimal)f.Quantity,
                    CurrentStock = (decimal)f.CurrentStock,
                    CostPerKg = (decimal)f.CostPerKg
                })
                .ToListAsync();

            var result = new PaginatedList<FoodStackItemModel>(items, totalItems, filter.PageNumber, filter.PageSize);

            return new PagedResult<FoodStackItemModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage,
            };
        }

        public async Task<PagedResult<StockLogItemModel>> GetStockLogHistoryAsync(Guid foodStackId, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.StockLogs
             .FindByCondition(sl => sl.StackId == foodStackId, false)
             .AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(sl => sl.DateAdded) // Sắp xếp theo ngày thêm vào
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(sl => new StockLogItemModel
                {
                    Id = sl.Id,
                    FoodType = sl.FoodType,
                    Quantity = (decimal)sl.Quantity,
                    CostPerKg = (decimal)sl.CostPerKg,
                    DateAdded = sl.DateAdded
                })
                .ToListAsync();

            var result = new PaginatedList<StockLogItemModel>(items, totalItems, pageNumber, pageSize);

            return new PagedResult<StockLogItemModel>
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
    }
}
