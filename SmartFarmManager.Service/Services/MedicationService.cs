using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Medication;
using SmartFarmManager.Service.Helpers;
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

        public async Task<PagedResult<MedicationModel>> GetMedicationsAsync(MedicationFilterModel filter)
        {
            var query = _unitOfWork.Medication
                .FindAll(false) 
                .AsQueryable();

            // Tìm kiếm theo tên thuốc nếu có
            if (!string.IsNullOrEmpty(filter.KeySearch))
            {
                query = query.Where(m => m.Name.Contains(filter.KeySearch));
            }

            // Lọc theo giá nếu có
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(m => m.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(m => m.Price <= filter.MaxPrice.Value);
            }

            // Lọc theo DoseWeight nếu có
            if (filter.MinDoseWeight.HasValue)
            {
                query = query.Where(m => m.DoseWeight >= filter.MinDoseWeight.Value);
            }

            if (filter.MaxDoseWeight.HasValue)
            {
                query = query.Where(m => m.DoseWeight <= filter.MaxDoseWeight.Value);
            }

            // Lọc theo Weight nếu có
            if (filter.MinWeight.HasValue)
            {
                query = query.Where(m => m.Weight >= filter.MinWeight.Value);
            }

            if (filter.MaxWeight.HasValue)
            {
                query = query.Where(m => m.Weight <= filter.MaxWeight.Value);
            }
            if (filter.MinDoseQuantity.HasValue)
            {
                query = query.Where(m => m.DoseQuantity >= filter.MinDoseQuantity.Value);
            }

            if (filter.MaxDoseQuantity.HasValue)
            {
                query = query.Where(m => m.DoseQuantity <= filter.MaxDoseQuantity.Value);
            }

            // Lọc theo PricePerDose nếu có
            if (filter.MinPricePerDose.HasValue)
            {
                query = query.Where(m => m.PricePerDose >= filter.MinPricePerDose.Value);
            }

            if (filter.MaxPricePerDose.HasValue)
            {
                query = query.Where(m => m.PricePerDose <= filter.MaxPricePerDose.Value);
            }
            if(filter.IsDeleted.HasValue)
            {
                query = query.Where(m => m.IsDeleted == filter.IsDeleted);
            }

            // Lấy tổng số thuốc sau khi lọc
            var totalItems = await query.CountAsync();

            // Phân trang và lấy dữ liệu
            var medications = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(m => new MedicationModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    UsageInstructions = m.UsageInstructions,
                    Price = m.Price,
                    DoseWeight = m.DoseWeight,
                    Weight = m.Weight,
                    DoseQuantity = m.DoseQuantity,
                    PricePerDose = m.PricePerDose,
                    IsDeleted = m.IsDeleted
                })
                .ToListAsync();

            var result = new PaginatedList<MedicationModel>(medications, totalItems, filter.PageNumber, filter.PageSize);

            return new PagedResult<MedicationModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage,
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
            var medication = await _unitOfWork.Medication
                .FindByCondition(m => m.Id == id)
                .FirstOrDefaultAsync();
            if (medication == null)
            {
                throw new KeyNotFoundException($"Thuốc với  ID {id} không tồn tại");
            }
            if (medication.IsDeleted)
            {
                var medicationExisting = await _unitOfWork.Medication
                    .FindByCondition(m => m.Name == medication.Name&&m.IsDeleted==false)
                    .FirstOrDefaultAsync();
                if (medicationExisting != null)
                {
                    throw new ArgumentException($"Thuốc với tên {medication.Name} đã tồn tại nên không thể khôi phục được! ");
                }
                medication.IsDeleted = false;
            }
            else
            {
                medication.IsDeleted = true;
            }
            await _unitOfWork.Medication.UpdateAsync(medication);
            await _unitOfWork.CommitAsync();

            if (medication.IsDeleted)
            {
                return true;
            }
            else
            {
                return false;
            }
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
