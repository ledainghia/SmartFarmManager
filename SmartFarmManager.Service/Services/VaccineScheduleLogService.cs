using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.VaccineScheduleLog;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class VaccineScheduleLogService : IVaccineScheduleLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public VaccineScheduleLogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Guid?> CreateVaccineScheduleLogAsync(Guid cageId, VaccineScheduleLogModel model)
        {
            // Tìm FarmingBatch với trạng thái "đang diễn ra"
            var farmingBatch = await _unitOfWork.FarmingBatches.FindByCondition(
                fb => fb.CageId == cageId && fb.Status == FarmingBatchStatusEnum.Active,
                trackChanges: false
            ).FirstOrDefaultAsync();

            if (farmingBatch == null)
                return null;

            // Tìm GrowthStage với trạng thái "đang diễn ra"
            var growthStage = await _unitOfWork.GrowthStages.FindByCondition(
                gs => gs.FarmingBatchId == farmingBatch.Id && gs.Status == GrowthStageStatusEnum.Active,
                trackChanges: false
            ).FirstOrDefaultAsync();

            if (growthStage == null)
                return null;

            // Tìm VaccineSchedule theo ngày hiện tại
            var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var vaccineSchedule = await _unitOfWork.VaccineSchedules.FindByCondition(
                vs => vs.StageId == growthStage.Id && DateOnly.FromDateTime(vs.Date.Value) == currentDate,
                trackChanges: false
            ).FirstOrDefaultAsync();

            if (vaccineSchedule == null)
                return null;

            // Tạo log
            var newLog = new VaccineScheduleLog
            {
                ScheduleId = vaccineSchedule.Id,
                Notes = model.Notes,
                Photo = model.Photo,
                Date = currentDate
            };

            await _unitOfWork.VaccineScheduleLogs.CreateAsync(newLog);
            await _unitOfWork.CommitAsync();

            return newLog.Id;
        }


        public async Task<VaccineScheduleLogModel> GetVaccineScheduleLogByIdAsync(Guid id)
        {
            var log = await _unitOfWork.VaccineScheduleLogs.GetByIdAsync(id);
            return _mapper.Map<VaccineScheduleLogModel>(log);
        }
    }
}
