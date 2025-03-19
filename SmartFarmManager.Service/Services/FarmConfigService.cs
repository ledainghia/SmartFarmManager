using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MimeKit.Utils;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.FarmConfig;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class FarmConfigService:IFarmConfigService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaskService _taskService; 

        public FarmConfigService(IUnitOfWork unitOfWork, ITaskService taskService)
        {
            _unitOfWork = unitOfWork;
            _taskService = taskService;
        }

        public async Task UpdateFarmTimeDifferenceAsync(Guid farmId, DateTime newTime)
        {
            var farmConfig = await _unitOfWork.FarmConfigs.FindByCondition(f => f.FarmId == farmId).FirstOrDefaultAsync();

            if (farmConfig == null)
            {
                throw new Exception("Farm configuration not found.");
            }

            DateTime currentTime = DateTimeUtils.VietnamNow();

            TimeSpan timeDifference = newTime - currentTime;

            farmConfig.TimeDifferenceInMinutes = (int)timeDifference.TotalMinutes ;
            farmConfig.LastTimeUpdated = DateTime.UtcNow;

            await _unitOfWork.FarmConfigs.UpdateAsync(farmConfig);
            await _unitOfWork.CommitAsync();

            DateTimeUtils.SetTimeDifference(farmConfig.TimeDifferenceInMinutes);
            
        }

        public async Task ResetTimeDifferenceAsync(Guid farmId)
        {
            var farmConfig = await _unitOfWork.FarmConfigs.FindByCondition(f => f.FarmId == farmId).FirstOrDefaultAsync();
            if (farmConfig == null)
            {
                throw new Exception("Farm configuration not found.");
            }
            farmConfig.TimeDifferenceInMinutes = 0;
            farmConfig.LastTimeUpdated = DateTime.UtcNow;
            await _unitOfWork.FarmConfigs.UpdateAsync(farmConfig);
            await _unitOfWork.CommitAsync();
            DateTimeUtils.SetTimeDifference(farmConfig.TimeDifferenceInMinutes);
            _taskService.UpdateAllTaskStatusesAsync();
        }

        public async Task<bool> UpdateFarmConfigAsync(Guid farmId, FarmConfigUpdateModel model)
        {
            var farmConfig = await _unitOfWork.FarmConfigs.FindByCondition(fc => fc.FarmId == farmId).FirstOrDefaultAsync();

            if (farmConfig == null)
            {
                throw new ArgumentException($"Farm config with FarmId {farmId} not found.");
            }

            if (model.MaxCagesPerStaff.HasValue)
            {
                if (model.MaxCagesPerStaff.Value <= 0)
                {
                    throw new ArgumentException("MaxCagesPerStaff must be greater than 0.");
                }
                farmConfig.MaxCagesPerStaff = model.MaxCagesPerStaff.Value;
            }

            if (model.MaxFarmingBatchesPerCage.HasValue)
            {
                if (model.MaxFarmingBatchesPerCage.Value <= 0)
                {
                    throw new ArgumentException("MaxFarmingBatchesPerCage must be greater than 0.");
                }
                farmConfig.MaxFarmingBatchesPerCage = model.MaxFarmingBatchesPerCage.Value;
            }
            farmConfig.LastTimeUpdated = DateTime.UtcNow;
            await _unitOfWork.FarmConfigs.UpdateAsync(farmConfig);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<FarmConfigItemModel> GetFarmConfigByFarmIdAsync(Guid farmId)
        {
            var farmConfig = await _unitOfWork.FarmConfigs.FindByCondition(fc => fc.FarmId == farmId).FirstOrDefaultAsync();
            if (farmConfig == null)
            {
                throw new ArgumentException($"Farm config with FarmId {farmId} not found.");
            }
            return new FarmConfigItemModel
            {
                FarmId = farmConfig.FarmId,
                MaxCagesPerStaff = farmConfig.MaxCagesPerStaff,
                MaxFarmingBatchesPerCage = farmConfig.MaxFarmingBatchesPerCage,               
            };
        }



    }
}
