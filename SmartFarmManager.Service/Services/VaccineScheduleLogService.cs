using AutoMapper;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.VaccineScheduleLog;
using SmartFarmManager.Service.Interfaces;
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

        public async Task<Guid> CreateVaccineScheduleLogAsync(VaccineScheduleLogModel model)
        {
            var log = _mapper.Map<VaccineScheduleLog>(model);
            await _unitOfWork.VaccineScheduleLogs.CreateAsync(log);
            await _unitOfWork.CommitAsync();
            return log.Id;
        }

        public async Task<VaccineScheduleLogModel> GetVaccineScheduleLogByIdAsync(Guid id)
        {
            var log = await _unitOfWork.VaccineScheduleLogs.GetByIdAsync(id);
            return _mapper.Map<VaccineScheduleLogModel>(log);
        }
    }
}
