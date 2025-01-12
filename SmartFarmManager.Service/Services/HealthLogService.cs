using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.HealthLog;
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
    public class HealthLogService : IHealthLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HealthLogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Guid?> CreateHealthLogAsync(Guid prescriptionId, HealthLogModel model)
        {
            // Tìm Prescription với CageId và trạng thái phù hợp
            var prescription = await _unitOfWork.Prescription
                .FindByCondition(p => p.Id == prescriptionId && p.Status == PrescriptionStatusEnum.Active)
                .Include(p => p.PrescriptionMedications)
                .Include(p => p.MedicalSymtom)
                .ThenInclude(p => p.FarmingBatch)
                .FirstOrDefaultAsync();
            if (prescription == null)
                return null;
            // Kiểm tra đơn thuốc có thuốc kê cho các buổi sáng, trưa, chiều, tối hay không
            var hasMorningMedication = prescription.PrescriptionMedications.Any(m => m.Morning > 0);
            var hasNoonMedication = prescription.PrescriptionMedications.Any(m => m.Noon > 0);
            var hasAfternoonMedication = prescription.PrescriptionMedications.Any(m => m.Afternoon > 0);
            var hasEveningMedication = prescription.PrescriptionMedications.Any(m => m.Evening > 0);

            // Lấy thời gian hiện tại
            var now = DateTimeUtils.VietnamNow();
            var currentTime = now.TimeOfDay;
            var currentSession = SessionTime.GetCurrentSession(currentTime);

            // Nếu ngày hiện tại trùng với EndDate và là buổi cuối cùng được kê thuốc
            if (prescription.EndDate.HasValue && now.Date == prescription.EndDate.Value.Date)
            {
                var isLastSession = currentSession switch
                {
                    1 => !hasNoonMedication && !hasAfternoonMedication && !hasEveningMedication,  // Morning là buổi cuối
                    2 => !hasAfternoonMedication && !hasEveningMedication,                      // Noon là buổi cuối
                    3 => !hasEveningMedication,                                                // Afternoon là buổi cuối
                    4 => true,                                                                 // Evening là buổi cuối
                    _ => false
                };

                if (isLastSession)
                {
                    // Cập nhật trạng thái Prescription thành Completed
                    prescription.Status = PrescriptionStatusEnum.Completed;
                    await _unitOfWork.Prescription.UpdateAsync(prescription);
                    var farmingBatch = await _unitOfWork.FarmingBatches.FindByCondition(f => f.Id == prescription.MedicalSymtom.FarmingBatchId).FirstOrDefaultAsync();
                    if (farmingBatch != null) {
                        farmingBatch.AffectedQuantity -= prescription.QuantityAnimal;
                        await _unitOfWork.FarmingBatches.UpdateAsync(farmingBatch);
                    }
                    else
                    {
                        return null;
                    }
                }
                else {
                    return null;
                }
            }
            // Tạo log
            var newLog = new HealthLog
            {
                PrescriptionId = prescription.Id,
                Date = DateTimeUtils.VietnamNow(),
                Notes = model.Notes,
                Photo = model.Photo,
                TaskId = model.TaskId
            };

            await _unitOfWork.HealthLogs.CreateAsync(newLog);
            await _unitOfWork.CommitAsync();

            return newLog.Id;
        }


        public async Task<HealthLogModel> GetHealthLogByIdAsync(Guid id)
        {
            var healthLog = await _unitOfWork.HealthLogs.GetByIdAsync(id, hl => hl.Prescription);
            if (healthLog == null) return null;
            return _mapper.Map<HealthLogModel>(healthLog);
        }

        public async Task<IEnumerable<HealthLogModel>> GetHealthLogsAsync(Guid? prescriptionId)
        {
            var query = _unitOfWork.HealthLogs.FindAll(false, hl => hl.Prescription);

            if (prescriptionId.HasValue)
                query = query.Where(hl => hl.PrescriptionId == prescriptionId);

            var healthLogs = await query.ToListAsync();
            return _mapper.Map<IEnumerable<HealthLogModel>>(healthLogs);
        }

        public async Task<HealthLogModel?> GetHealthLogByTaskIdAsync(Guid taskId)
        {
            // Tìm log Health dựa trên TaskId
            var healthLog = await _unitOfWork.HealthLogs.FindByCondition(
                log => log.TaskId == taskId,
                trackChanges: false
            ).FirstOrDefaultAsync();

            if (healthLog == null)
                return null;

            return new HealthLogModel
            {
                Id = healthLog.Id,
                PrescriptionId = healthLog.PrescriptionId,
                Date = healthLog.Date,
                Notes = healthLog.Notes,
                Photo = healthLog.Photo,
                TaskId = healthLog.TaskId
            };
        }

    }
}
