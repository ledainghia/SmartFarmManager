using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MimeKit.Utils;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class FarmConfigService:IFarmConfigService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FarmConfigService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task UpdateFarmTimeDifferenceAsync(Guid farmId, DateTime newTime)
        {
            var farmConfig = await _unitOfWork.FarmConfigs.FindByCondition(f => f.FarmId == farmId).FirstOrDefaultAsync();

            if (farmConfig == null)
            {
                throw new Exception("Farm configuration not found.");
            }

            DateTime currentTime = DateTimeUtils.GetServerTimeInVietnamTime();

            TimeSpan timeDifference = newTime - currentTime;

            farmConfig.TimeDifference = timeDifference;
            farmConfig.LastTimeUpdated = DateTime.UtcNow;

            await _unitOfWork.FarmConfigs.UpdateAsync(farmConfig);
            await _unitOfWork.CommitAsync();

            DateTimeUtils.SetTimeDifference(farmConfig.TimeDifference);
        }

        public async Task ResetTimeDifferenceAsync(Guid farmId)
        {
            var farmConfig = await _unitOfWork.FarmConfigs.FindByCondition(f => f.FarmId == farmId).FirstOrDefaultAsync();
            if (farmConfig == null)
            {
                throw new Exception("Farm configuration not found.");
            }
            farmConfig.TimeDifference = TimeSpan.Zero;
            farmConfig.LastTimeUpdated = DateTime.UtcNow;
            await _unitOfWork.FarmConfigs.UpdateAsync(farmConfig);
            await _unitOfWork.CommitAsync();
            DateTimeUtils.SetTimeDifference(farmConfig.TimeDifference);
        }



    }
}
