using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.AnimalSale;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class AnimalSaleService : IAnimalSaleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AnimalSaleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateAnimalSaleAsync(CreateAnimalSaleRequest request)
        {
            var growthStage = await _unitOfWork.GrowthStages
                .FindByCondition(gs => gs.Id == request.GrowthStageId)
                .Include(gs => gs.FarmingBatch)
                .FirstOrDefaultAsync();

            if (growthStage == null)
                return false;

            var newAnimalSale = new AnimalSale
            {
                Id = Guid.NewGuid(),
                FarmingBatchId = growthStage.FarmingBatchId,
                SaleDate = request.SaleDate,
                Total = request.UnitPrice * request.Quantity,
                UnitPrice = request.UnitPrice,
                Quantity = request.Quantity,
                StaffId = request.StaffId,
                SaleTypeId = request.SaleTypeId
            };

            await _unitOfWork.AnimalSales.CreateAsync(newAnimalSale);
            await _unitOfWork.CommitAsync();
            return true;
        }

    }
}