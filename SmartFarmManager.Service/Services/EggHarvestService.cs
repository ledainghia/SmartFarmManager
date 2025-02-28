using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.EggHarvest;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class EggHarvestService : IEggHarvestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EggHarvestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// 📌 **Tạo EggHarvest**
        public async Task<bool> CreateEggHarvestAsync(CreateEggHarvestRequest request)
        {
            // Kiểm tra GrowthStage tồn tại không
            var growthStage = await _unitOfWork.GrowthStages
                .FindByCondition(gs => gs.Id == request.GrowthStageId)
                .FirstOrDefaultAsync();

            if (growthStage == null)
            {
                throw new ArgumentException("GrowthStage not found.");
            }

            var newEggHarvest = new EggHarvest
            {
                Id = Guid.NewGuid(),
                GrowthStageId = request.GrowthStageId,
                DateCollected = DateTimeUtils.GetServerTimeInVietnamTime(),
                EggCount = request.EggCount,
                Notes = request.Notes,
                TaskId = request.TaskId
            };

            await _unitOfWork.EggHarvests.CreateAsync(newEggHarvest);
            await _unitOfWork.CommitAsync();

            return true;
        }

        /// 📌 **Lấy danh sách EggHarvest theo TaskId**
        public async Task<IEnumerable<EggHarvestResponse>> GetEggHarvestsByTaskIdAsync(Guid taskId)
        {
            var eggHarvests = await _unitOfWork.EggHarvests
                .FindByCondition(eh => eh.TaskId == taskId)
                .Include(eh => eh.growthStage)
                .ToListAsync();

            return eggHarvests.Select(eh => new EggHarvestResponse
            {
                Id = eh.Id,
                GrowthStageId = eh.GrowthStageId,
                DateCollected = eh.DateCollected,
                EggCount = eh.EggCount,
                Notes = eh.Notes,
                TaskId = eh.TaskId,
                GrowthStageName = eh.growthStage?.Name
            }).ToList();
        }
    }
}
