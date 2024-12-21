using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.HealthLog;
using SmartFarmManager.Service.Interfaces;
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

        public async Task<Guid> CreateHealthLogAsync(HealthLogModel model)
        {
            var task = await _unitOfWork.Tasks.FindByCondition(p => p.Id == model.TaskId).FirstOrDefaultAsync(); 
            var prescription = await _unitOfWork.Prescription.FindByCondition(p => p.CageId == task.CageId).FirstOrDefaultAsync();
            model.PrescriptionId = prescription.Id;
            var healthLog = _mapper.Map<HealthLog>(model);
            await _unitOfWork.HealthLogs.CreateAsync(healthLog);
            await _unitOfWork.CommitAsync();
            return healthLog.Id;
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
    }
}
