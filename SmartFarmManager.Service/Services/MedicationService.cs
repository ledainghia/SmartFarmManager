using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Medication;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MedicationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MedicationModel?> CreateMedicationAsync(MedicationModel medicationModel)
        {
            if (medicationModel == null)
            {
                throw new ArgumentNullException(nameof(medicationModel), "Medication model cannot be null.");
            }
            var medication = new Medication
            {
                Name = medicationModel.Name,
                UsageInstructions = medicationModel.UsageInstructions,
                Price = medicationModel.Price,
                DoseQuantity = medicationModel.DoseQuantity,
                PricePerDose = medicationModel.PricePerDose
            };

            await _unitOfWork.Medication.CreateAsync(medication);
            await _unitOfWork.CommitAsync();

            // Map back to MedicationModel to return
            return new MedicationModel
            {
                Id = medication.Id,
                Name = medication.Name,
                UsageInstructions = medication.UsageInstructions,
                Price = medication.Price,
                DoseQuantity = medication.DoseQuantity,
                PricePerDose = medication.PricePerDose
            };
        }

        public async Task<MedicationModel?> GetMedicationByName(string medicationName)
        {
            var medication = await _unitOfWork.Medication.FindByCondition(m => m.Name.Equals(medicationName)).FirstOrDefaultAsync();
            return _mapper.Map<MedicationModel?>(medication);
        }


        public async Task<IEnumerable<MedicationModel>> GetAllMedicationsAsync()
        {
            var medication = await _unitOfWork.Medication.FindAll().ToListAsync();
            return _mapper.Map<List<MedicationModel>>(medication);
        }

        public async Task<PagedResult<MedicationModel>> GetPagedMedicationsAsync(string? name, decimal? minPrice, decimal? maxPrice, int page, int pageSize)
        {
            var (items, totalCount) = await _unitOfWork.Medication.GetPagedAsync(
                filter: m =>
                    (string.IsNullOrEmpty(name) || m.Name.Contains(name)) &&
                    (!minPrice.HasValue || m.Price >= minPrice) &&
                    (!maxPrice.HasValue || m.Price <= maxPrice),
                orderBy: q => q.OrderBy(m => m.Name),
                page: page,
                pageSize: pageSize
            );

            return new PagedResult<MedicationModel>
            {
                Items = items.Select(m => new MedicationModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    UsageInstructions = m.UsageInstructions,
                    Price = m.Price,
                    DoseQuantity = m.DoseQuantity,
                    PricePerDose = m.PricePerDose
                }),
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                HasNextPage = page < (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = page > 1
            };
        }
        public async Task<bool> UpdateMedicationAsync(Guid id, UpdateMedicationModel model)
        {
            var existingMedication = await _unitOfWork.Medication
                .FindByCondition(m => m.Id == id)
                .FirstOrDefaultAsync();

            if (existingMedication == null)
            {
                throw new KeyNotFoundException($"Medication with ID {id} does not exist.");
            }
            if (await _unitOfWork.Medication.AnyAsync(x => x.Name.Equals(model.Name))){
                throw new ArgumentException($"Medication with name {model.Name} already exists.");
            }

            existingMedication.Name = model.Name ?? existingMedication.Name;
            existingMedication.UsageInstructions = model.UsageInstructions ?? existingMedication.UsageInstructions;
            existingMedication.Price = model.Price ?? existingMedication.Price;
            existingMedication.DoseWeight = model.DoseWeight ?? existingMedication.DoseWeight;
            existingMedication.Weight = model.Weight ?? existingMedication.Weight;
            existingMedication.DoseQuantity = model.DoseQuantity ?? existingMedication.DoseQuantity;
            existingMedication.PricePerDose = model.PricePerDose ?? existingMedication.PricePerDose;

            await _unitOfWork.Medication.UpdateAsync(existingMedication);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<bool> DeleteMedicationAsync(Guid id)
        {
            // 1. Kiểm tra Medication có tồn tại không
            var medication = await _unitOfWork.Medication
                .FindByCondition(m => m.Id == id)
                .FirstOrDefaultAsync();

            if (medication == null)
            {
                throw new KeyNotFoundException($"Medication with ID {id} does not exist.");
            }

            await _unitOfWork.Medication.DeleteAsync(medication);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<MedicationDetailResponseModel?> GetMedicationDetailAsync(Guid id)
        {
            var medication = await _unitOfWork.Medication
                .FindByCondition(m => m.Id == id)
                .FirstOrDefaultAsync();

            if (medication == null)
            {
                return null;
            }

            return new MedicationDetailResponseModel
            {
                Id = medication.Id,
                Name = medication.Name,
                UsageInstructions = medication.UsageInstructions,
                Price = medication.Price,
                DoseWeight = medication.DoseWeight,
                Weight = medication.Weight,
                DoseQuantity = medication.DoseQuantity,
                PricePerDose = medication.PricePerDose
            };
        }


    }

}
