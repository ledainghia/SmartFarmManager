using AutoMapper;
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

        public async Task<Guid> CreateDailyFoodUsageLogAsync(DailyFoodUsageLogModel model)
        {
            var log = _mapper.Map<DailyFoodUsageLog>(model);
            await _unitOfWork.DailyFoodUsageLogs.CreateAsync(log);
            await _unitOfWork.CommitAsync();
            return log.Id;
        }

        public async Task<DailyFoodUsageLogModel> GetDailyFoodUsageLogByIdAsync(Guid id)
        {
            var log = await _unitOfWork.DailyFoodUsageLogs.GetByIdAsync(id);
            return _mapper.Map<DailyFoodUsageLogModel>(log);
        }
    }

    
}
