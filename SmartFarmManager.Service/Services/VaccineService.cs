using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Vaccine;
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
    public class VaccineService : IVaccineService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VaccineService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VaccineModel> GetActiveVaccineByCageIdAsync(Guid cageId)
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
            var currentDate = DateOnly.FromDateTime(DateTimeUtils.VietnamNow());
            var vaccineSchedule = await _unitOfWork.VaccineSchedules.FindByCondition(
                vs => vs.StageId == growthStage.Id && DateOnly.FromDateTime(vs.Date.Value) == currentDate,
                trackChanges: false
            ).FirstOrDefaultAsync();

            if (vaccineSchedule == null)
                return null;

            // Lấy Vaccine dựa trên VaccineSchedule
            var vaccine = await _unitOfWork.Vaccines.FindByCondition(
                v => v.Id == vaccineSchedule.VaccineId,
                trackChanges: false
            ).FirstOrDefaultAsync();

            if (vaccine == null)
                return null;

            // Map Vaccine sang VaccineModel
            return new VaccineModel
            {
                Id = vaccine.Id,
                Name = vaccine.Name,
                Method = vaccine.Method,
                AgeStart = vaccine.AgeStart,
                AgeEnd = vaccine.AgeEnd
            };
        }

    }
}
