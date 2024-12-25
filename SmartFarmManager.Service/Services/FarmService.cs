using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Farm;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class FarmService : IFarmService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FarmService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> CreateFarmAsync(FarmModel model)
        {
            var farm = new Farm
            {
                Name = model.Name,
                Address = model.Address,
                Area = model.Area,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                CreatedDate = DateTimeUtils.VietnamNow()
            };

            var id = await _unitOfWork.Farms.CreateAsync(farm);
            await _unitOfWork.CommitAsync();
            return id;
        }

        public async Task<FarmModel> GetFarmByIdAsync(Guid id)
        {
            var farm = await _unitOfWork.Farms.GetByIdAsync(id);
            if (farm == null) return null;

            return new FarmModel
            {
                Id = farm.Id,
                Name = farm.Name,
                Address = farm.Address,
                Area = farm.Area,
                PhoneNumber = farm.PhoneNumber,
                Email = farm.Email
            };
        }

        public async Task<IEnumerable<FarmModel>> GetAllFarmsAsync(string? search)
        {
            var farms = await _unitOfWork.Farms.FindAllAsync(f => string.IsNullOrEmpty(search) || f.Name.Contains(search));

            return farms.Select(f => new FarmModel
            {
                Id = f.Id,
                Name = f.Name,
                Address = f.Address,
                Area = f.Area,
                PhoneNumber = f.PhoneNumber,
                Email = f.Email
            });
        }

        public async Task<bool> UpdateFarmAsync(Guid id, FarmModel model)
        {
            var farm = await _unitOfWork.Farms.GetByIdAsync(id);
            if (farm == null) return false;

            farm.Name = model.Name;
            farm.Address = model.Address;
            farm.Area = model.Area;
            farm.PhoneNumber = model.PhoneNumber;
            farm.Email = model.Email;
            farm.ModifiedDate = DateTimeUtils.VietnamNow();

            await _unitOfWork.Farms.UpdateAsync(farm);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteFarmAsync(Guid id)
        {
            var farm = await _unitOfWork.Farms.GetByIdAsync(id);
            if (farm == null) return false;

            farm.IsDeleted = true;
            farm.DeletedDate = DateTimeUtils.VietnamNow();

            await _unitOfWork.Farms.UpdateAsync(farm);
            await _unitOfWork.CommitAsync();
            return true;
        }
    }

}
