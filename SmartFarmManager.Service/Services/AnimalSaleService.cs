using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.AnimalSale;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class AnimalSaleService : IAnimalSaleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AnimalSaleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateAnimalSaleAsync(CreateAnimalSaleRequest request)
        {
            var growthStage = await _unitOfWork.GrowthStages
                .FindByCondition(gs => gs.Id == request.GrowthStageId)
                .Include(gs => gs.FarmingBatch)
                .FirstOrDefaultAsync();

            if (growthStage == null)
                return false;

            var staff = await _unitOfWork.Users
                .FindByCondition(s => s.Id == request.StaffId)
                .FirstOrDefaultAsync();
            if (staff ==null )
            {
                return false;
            }
            var typeSale = await _unitOfWork.SaleTypes
                .FindByCondition(s => s.Id == request.SaleTypeId)
                .FirstOrDefaultAsync();
            var newAnimalSale = new AnimalSale
            {
                Id = Guid.NewGuid(),
                FarmingBatchId = growthStage.FarmingBatchId,
                SaleDate = request.SaleDate,
                Total = request.UnitPrice * request.Quantity,
                UnitPrice = request.UnitPrice,
                Quantity = request.Quantity,
                StaffId = request.StaffId,
                SaleTypeId = request.SaleTypeId
            };
            await _unitOfWork.AnimalSales.CreateAsync(newAnimalSale);
            var newAnimalSaleLogByTask = new AnimalSaleLogByTaskModel
            {
                GrowthStageId = request.GrowthStageId,
                GrowthStageName = growthStage.Name,
                LogTime =DateTimeUtils.GetServerTimeInVietnamTime(),                
                Quantity = request.Quantity,
                SaleDate = request.SaleDate,
                SaleTypeId = request.SaleTypeId,
                SaleTypeName = typeSale.StageTypeName,
                StaffId = request.StaffId,
                StaffName=staff.FullName,
                Total = request.UnitPrice * request.Quantity,
                UnitPrice = request.UnitPrice               
            };
            var task = await _unitOfWork.Tasks.FindByCondition(t => t.Id == request.TaskId).FirstOrDefaultAsync();
            if (task != null)
            {
                var statusLog = new StatusLog
                {
                    TaskId = task.Id,
                    UpdatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                    Status = TaskStatusEnum.Done,  
                    Log = JsonConvert.SerializeObject(newAnimalSaleLogByTask)  
                };

                await _unitOfWork.StatusLogs.CreateAsync(statusLog); 
            }
            await _unitOfWork.CommitAsync();
            return true;
        }


        public async Task<AnimalSaleLogByTaskModel> GetAnimalSaleLogByTaskId(Guid taskId)
        {
            try
            {
                // Lấy task theo taskId và bao gồm các status log
                var task = await _unitOfWork.Tasks
                    .FindByCondition(t => t.Id == taskId)
                    .Include(t => t.StatusLogs)
                    .Include(t=>t.TaskType)
                    .FirstOrDefaultAsync();

                if (task == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy task với ID '{taskId}'.");
                }

                if (task.TaskType?.TaskTypeName != "Bán vật nuôi")
                {
                    throw new InvalidOperationException("Task không phải là 'Bán vật nuôi'.");
                }

                var statusLog = task.StatusLogs
                    .FirstOrDefault(sl => sl.Status == TaskStatusEnum.Done);

                if (statusLog == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy StatusLog với trạng thái 'Done'.");
                }

                if (string.IsNullOrEmpty(statusLog.Log))
                {
                    throw new InvalidOperationException("Log của task này không có dữ liệu.");
                }
                var animalSaleLog = JsonConvert.DeserializeObject<AnimalSaleLogByTaskModel>(statusLog.Log);

                return animalSaleLog;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy log từ task: {ex.Message}");
            }
        }


    }
}