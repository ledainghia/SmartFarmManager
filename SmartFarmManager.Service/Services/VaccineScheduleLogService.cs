using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.VaccineScheduleLog;
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
            var currentDate = DateOnly.FromDateTime(DateTimeUtils.GetServerTimeInVietnamTime());
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
                Date = currentDate,
                TaskId = model.TaskId
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

        public async Task<VaccineScheduleLogModel> GetVaccineScheduleLogByTaskIdAsync(Guid taskId)
        {
            // Tìm VaccineScheduleLog dựa trên TaskId
            var log = await _unitOfWork.VaccineScheduleLogs.FindByCondition(
                log => log.TaskId == taskId,
                trackChanges: false
            ).FirstOrDefaultAsync();

            if (log == null)
                return null;

            return new VaccineScheduleLogModel
            {
                Id = log.Id,
                ScheduleId = log.ScheduleId,
                Date = log.Date,
                Notes = log.Notes,
                Photo = log.Photo,
                TaskId = log.TaskId
            };
        }
        public async Task<bool> CreateVaccineLogAsync(CreateVaccineLogRequest request)
        {
            // 1️⃣ Tìm VaccineSchedule theo Date, Session, VaccineId
            var vaccineSchedule = await _unitOfWork.VaccineSchedules
                .FindByCondition(vs => vs.Date.Value.Date == request.Date.Date &&
                                       vs.Session == request.Session &&
                                       vs.VaccineId == request.VaccineId)
                .FirstOrDefaultAsync();

            if (vaccineSchedule == null)
                throw new ArgumentException("No matching VaccineSchedule found.");

            // 2️⃣ Lấy thông tin giá vaccine
            var vaccine = await _unitOfWork.Vaccines.FindByCondition(v => v.Id == request.VaccineId).FirstOrDefaultAsync();
            if (vaccine == null)
                throw new ArgumentException("Vaccine not found.");

            // 3️⃣ Tạo log mới trong VaccineScheduleLog
            var newLog = new VaccineScheduleLog
            {
                Id = Guid.NewGuid(),
                ScheduleId = vaccineSchedule.Id,
                Date = DateOnly.FromDateTime(request.Date),
                Notes = request.Notes,
                Photo = request.Photo,
                TaskId = request.TaskId
            };

            await _unitOfWork.VaccineScheduleLogs.CreateAsync(newLog);

            // 4️⃣ Cập nhật VaccineSchedule
            vaccineSchedule.Status = VaccineScheduleStatusEnum.Completed;
            vaccineSchedule.Quantity = request.Quantity;
            vaccineSchedule.ToltalPrice = request.Quantity * (decimal)vaccine.Price;

            await _unitOfWork.VaccineSchedules.UpdateAsync(vaccineSchedule);
            await _unitOfWork.CommitAsync();

            return true;
        }

    }
}
