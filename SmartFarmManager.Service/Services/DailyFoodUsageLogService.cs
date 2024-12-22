using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.DailyFoodUsageLog;
using SmartFarmManager.Service.BusinessModels.VaccineScheduleLog;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class DailyFoodUsageLogService : IDailyFoodUsageLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DailyFoodUsageLogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Guid?> CreateDailyFoodUsageLogAsync(Guid cageId, DailyFoodUsageLogModel model)
        {
            // Tìm FarmingBatch với trạng thái "đang diễn ra"
            var farmingBatch = await _unitOfWork.FarmingBatches.FindByCondition(
                fb => fb.CageId == cageId && fb.Status == "Đang diễn ra",
                trackChanges: false
            ).FirstOrDefaultAsync();

            if (farmingBatch == null)
                return null;

            // Tìm GrowthStage với trạng thái "đang diễn ra"
            var growthStage = await _unitOfWork.GrowthStages.FindByCondition(
                gs => gs.FarmingBatchId == farmingBatch.Id && gs.Status == "Đang diễn ra",
                trackChanges: false
            ).FirstOrDefaultAsync();

            if (growthStage == null)
                return null;

            // Tạo log
            var newLog = new DailyFoodUsageLog
            {
                StageId = growthStage.Id,
                RecommendedWeight = model.RecommendedWeight,
                ActualWeight = model.ActualWeight,
                Notes = model.Notes,
                Photo = model.Photo,
                LogTime = DateTime.UtcNow,
                TaskId = model.TaskId,
            };

            await _unitOfWork.DailyFoodUsageLogs.CreateAsync(newLog);
            await _unitOfWork.CommitAsync();

            return newLog.Id;
        }


        public async Task<DailyFoodUsageLogModel> GetDailyFoodUsageLogByIdAsync(Guid id)
        {
            var log = await _unitOfWork.DailyFoodUsageLogs.GetByIdAsync(id);
            return _mapper.Map<DailyFoodUsageLogModel>(log);
        }
    }

    
}
