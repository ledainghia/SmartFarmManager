using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.HealthLog;
using SmartFarmManager.Service.BusinessModels.LogInTask;
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
            var newLog = new HealthLog
            {
                PrescriptionId = prescription.Id,
                Date = DateTimeUtils.GetServerTimeInVietnamTime(),
                Notes = model.Notes,
                Photo = model.Photo,
                TaskId = model.TaskId
            };

            await _unitOfWork.HealthLogs.CreateAsync(newLog);

            var newHealthLogInTask = new HealthLogInTaskModel {
            PrescriptionId = prescription.Id,
            Notes = model.Notes,
            LogTime = DateTimeUtils.GetServerTimeInVietnamTime(),
            Photo = model.Photo,
            TaskId = model.TaskId
            };
            var task = await _unitOfWork.Tasks.FindByCondition(t => t.Id == model.TaskId).FirstOrDefaultAsync();
            if (task != null)
            {

                var statusLog = new StatusLog
                {
                    TaskId = task.Id,
                    UpdatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                    Status = TaskStatusEnum.Done,
                    Log = JsonConvert.SerializeObject(newHealthLogInTask)
                };
                await _unitOfWork.StatusLogs.CreateAsync(statusLog);
            }

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
