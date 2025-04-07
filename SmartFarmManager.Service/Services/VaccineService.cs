using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
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
            var currentDate = DateOnly.FromDateTime(DateTimeUtils.GetServerTimeInVietnamTime());
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

        public async Task<bool> CreateVaccineAsync(CreateVaccineModel model)
        {
            // Check for duplicate Vaccine
            var existingVaccine = await _unitOfWork.Vaccines
                .FindByCondition(v => v.Name == model.Name && v.IsDeleted==false)
                .FirstOrDefaultAsync();

            if (existingVaccine != null)
            {
                throw new ArgumentException($"Vaccine with name '{model.Name}' already exists.");
            }
            if(model.AgeStart > model.AgeEnd)
            {
                throw new ArgumentException("Age start must be less than or equal to age end.");
            }

            // Create Vaccine
            var vaccine = new Vaccine
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Method = model.Method,
                Price = model.Price,
                AgeStart = model.AgeStart,
                AgeEnd = model.AgeEnd
            };

            await _unitOfWork.Vaccines.CreateAsync(vaccine);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<bool> UpdateVaccineAsync(Guid id, VaccineUpdateModel model)
        {
            var vaccine = await _unitOfWork.Vaccines
                .FindByCondition(v => v.Id == id)
                .FirstOrDefaultAsync();

            if (vaccine == null)
            {
                throw new KeyNotFoundException($"Vaccine with ID {id} not found.");
            }
            if(model.AgeStart > model.AgeEnd)
            {
                throw new ArgumentException("Age start must be less than or equal to age end.");
            }
            var vaccineWithSameName = await _unitOfWork.Vaccines
                .FindByCondition(v => v.Name == model.Name && v.Id != id)
                .FirstOrDefaultAsync();
            if(vaccineWithSameName != null)
            {
                throw new ArgumentException($"Vaccine with name '{model.Name}' already exists.");
            }

            vaccine.Name = model.Name ?? vaccine.Name;
            vaccine.Method = model.Method ?? vaccine.Method;
            vaccine.Price = model.Price ?? vaccine.Price;
            vaccine.AgeStart = model.AgeStart ?? vaccine.AgeStart;
            vaccine.AgeEnd = model.AgeEnd ?? vaccine.AgeEnd;

            await _unitOfWork.Vaccines.UpdateAsync(vaccine);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<bool> DeleteVaccineAsync(Guid id)
        {
            var vaccine = await _unitOfWork.Vaccines
                .FindByCondition(v => v.Id == id)
                .FirstOrDefaultAsync();

            if (vaccine == null)
            {
                throw new KeyNotFoundException($"Vaccine with ID {id} not found.");
            }
            vaccine.IsDeleted = true;
            await _unitOfWork.Vaccines.UpdateAsync(vaccine);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<PagedResult<VaccineItemModel>> GetVaccinesAsync(VaccineFilterModel filter)
        {
            var query = _unitOfWork.Vaccines.FindAll(false).AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(v => v.Name.Contains(filter.Name));
            }

            if (filter.AgeStart.HasValue)
            {
                query = query.Where(v => v.AgeStart >= filter.AgeStart.Value);
            }

            if (filter.AgeEnd.HasValue)
            {
                query = query.Where(v => v.AgeEnd <= filter.AgeEnd.Value);
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(v => new VaccineItemModel
                {
                    Id = v.Id,
                    Name = v.Name,
                    Method = v.Method,
                    Price = v.Price,
                    AgeStart = v.AgeStart,
                    AgeEnd = v.AgeEnd
                })
                .ToListAsync();

            var result = new PaginatedList<VaccineItemModel>(items, totalItems, filter.PageNumber, filter.PageSize);
            return new PagedResult<VaccineItemModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage

            };
        }
        public async Task<VaccineDetailResponseModel?> GetVaccineDetailAsync(Guid id)
        {
            var vaccine = await _unitOfWork.Vaccines
                .FindByCondition(v => v.Id == id)
                .FirstOrDefaultAsync();

            if (vaccine == null)
            {
                return null;
            }

            return new VaccineDetailResponseModel
            {
                Id = vaccine.Id,
                Name = vaccine.Name,
                Method = vaccine.Method,
                Price = vaccine.Price,
                AgeStart = vaccine.AgeStart,
                AgeEnd = vaccine.AgeEnd
            };
        }



    }
}
