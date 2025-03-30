using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.StandardPrescription;
using SmartFarmManager.Service.BusinessModels.StandardPrescriptionMedication;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class StandardPrescriptionService : IStandardPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StandardPrescriptionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<StandardPrescriptionModel?> GetStandardPrescriptionsByDiseaseIdAsync(Guid diseaseId)
        {
            // Lấy danh sách StandardPrescription từ database
            var prescriptions = await _unitOfWork.StandardPrescriptions
                .FindByCondition(sp => sp.DiseaseId == diseaseId)
                .Include(sp => sp.StandardPrescriptionMedications)
                .ThenInclude(spm => spm.Medication)
                .FirstOrDefaultAsync();
            if (prescriptions == null)
            {
                return null;
            }
            // Chuyển đổi sang model
            return new StandardPrescriptionModel
            {
                Id = prescriptions.Id,
                Notes = prescriptions.Notes,
                DiseaseId = prescriptions.DiseaseId,
                RecommentDay = prescriptions.RecommendDay,
                Medications = prescriptions.StandardPrescriptionMedications.Select(spm => new StandardPrescriptionMedicationModel
                {
                    Id = spm.Id,
                    MedicationId = spm.MedicationId,
                    UsageInstructions=spm.Medication.UsageInstructions,
                    MedicationName = spm.Medication.Name,
                    Morning = spm.Morning,
                    Afternoon = spm.Afternoon,
                    Evening = spm.Evening,
                    Noon = spm.Noon
                }).ToList()
            };
        }
        public async Task<bool> CreateStandardPrescriptionAsync(CreateStandardPrescriptionModel model)
        {
            var existingPrescription = await _unitOfWork.StandardPrescriptions
                .FindByCondition(sp => sp.DiseaseId == model.DiseaseId &&sp.IsDeleted==false)
                .FirstOrDefaultAsync();

            if (existingPrescription != null)
            {
                throw new ArgumentException($"Prescription với cho bênh có Id là {model.DiseaseId} đã tồn tại");
            }

            var standardPrescription = new StandardPrescription
            {
                DiseaseId = model.DiseaseId,
                Notes = model.Notes,
                RecommendDay = model.RecommendDay
            };

            await _unitOfWork.StandardPrescriptions.CreateAsync(standardPrescription);
            await _unitOfWork.CommitAsync();

            foreach (var medication in model.StandardPrescriptionMedications)
            {
                var medicationExists = await _unitOfWork.Medication
                    .FindByCondition(m => m.Id == medication.MedicationId)
                    .AnyAsync();

                if (!medicationExists)
                {
                    throw new ArgumentException($"Thuốc với Id là {medication.MedicationId}  không tồn tại.");
                }

                var standardPrescriptionMedication = new StandardPrescriptionMedication
                {
                    PrescriptionId = standardPrescription.Id,
                    MedicationId = medication.MedicationId,
                    Morning = medication.Morning,
                    Noon = medication.Noon,
                    Afternoon = medication.Afternoon,
                    Evening = medication.Evening
                };

                await _unitOfWork.StandardPrescriptionMedications.CreateAsync(standardPrescriptionMedication);
            }

            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> UpdateStandardPrescriptionAsync(Guid id, UpdateStandardPrescriptionModel model)
        {
            var existingPrescription = await _unitOfWork.StandardPrescriptions
                .FindByCondition(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (existingPrescription == null)
            {
                throw new KeyNotFoundException($"Đơn thuốc mẫu với ID {id} không tồn tại.");
            }

            existingPrescription.Notes = model.Notes ?? existingPrescription.Notes;
            existingPrescription.RecommendDay = model.RecommendDay ?? existingPrescription.RecommendDay;

            if (model.StandardPrescriptionMedications != null)
            {
                var existingMedications = await _unitOfWork.StandardPrescriptionMedications
                    .FindByCondition(spm => spm.PrescriptionId == id)
                    .ToListAsync();

                foreach (var medication in existingMedications)
                {
                    await _unitOfWork.StandardPrescriptionMedications.DeleteAsync(medication);
                }
                foreach (var medication in model.StandardPrescriptionMedications)
                {
                    var medicationExists = await _unitOfWork.Medication
                        .FindByCondition(m => m.Id == medication.MedicationId)
                        .AnyAsync();

                    if (!medicationExists)
                    {
                        throw new ArgumentException($"Thuốc với ID {medication.MedicationId} không tồn tại.");
                    }
                    var standardPrescriptionMedication = new StandardPrescriptionMedication
                    {
                        PrescriptionId = existingPrescription.Id,
                        MedicationId = medication.MedicationId,
                        Morning = medication.Morning,
                        Noon = medication.Noon,
                        Afternoon = medication.Afternoon,
                        Evening = medication.Evening
                    };

                    await _unitOfWork.StandardPrescriptionMedications.CreateAsync(standardPrescriptionMedication);
                }
            }

            await _unitOfWork.StandardPrescriptions.UpdateAsync(existingPrescription);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> DeleteStandardPrescriptionAsync(Guid id)
        {
            var existingPrescription = await _unitOfWork.StandardPrescriptions
                .FindByCondition(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (existingPrescription == null)
            {
                throw new KeyNotFoundException($"Đơn thuốc mẫu với ID {id} không tồn tại.");
            }

            // Cập nhật trường IsDeleted để "xóa mềm"
            existingPrescription.IsDeleted = true;

            await _unitOfWork.StandardPrescriptions.UpdateAsync(existingPrescription);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<PagedResult<StandardPrescriptionItemModel>> GetStandardPrescriptionsAsync(StandardPrescriptionFilterModel filter)
        {
            var query = _unitOfWork.StandardPrescriptions
                .FindAll(false)
                .Include(p => p.Disease)
                .Include(p => p.StandardPrescriptionMedications)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.KeySearch))
            {
                query = query.Where(p => p.Disease.Name.Contains(filter.KeySearch) || p.Notes.Contains(filter.KeySearch));
            }

            if (filter.DiseaseId.HasValue)
            {
                query = query.Where(p => p.DiseaseId == filter.DiseaseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Notes))
            {
                query = query.Where(p => p.Notes.Contains(filter.Notes));
            }

            if (filter.RecommendDay.HasValue)
            {
                query = query.Where(p => p.RecommendDay == filter.RecommendDay.Value);
            }
            if(filter.IsDeleted.HasValue)
            {
                query = query.Where(p => p.IsDeleted == filter.IsDeleted.Value);
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new StandardPrescriptionItemModel
                {
                    Id = p.Id,
                    DiseaseName = p.Disease.Name,
                    Notes = p.Notes,
                    RecommendDay = p.RecommendDay,
                    IsDeleted = p.IsDeleted

                })
                .ToListAsync();

            var result = new PaginatedList<StandardPrescriptionItemModel>(items, totalItems, filter.PageNumber, filter.PageSize);

            return new PagedResult<StandardPrescriptionItemModel>
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

        public async Task<StandardPrescriptionDetailResponseModel> GetStandardPrescriptionDetailAsync(Guid id)
        {
            var prescription = await _unitOfWork.StandardPrescriptions
                .FindByCondition(p => p.Id == id && p.IsDeleted ==false)
                .Include(p => p.StandardPrescriptionMedications)
                    .ThenInclude(spm => spm.Medication)
                .Include(p => p.Disease)
                .FirstOrDefaultAsync();
            Console.Write(prescription);

            if (prescription == null)
            {
                return null;
            }

            return new StandardPrescriptionDetailResponseModel
            {
                Id = prescription.Id,
                DiseaseName = prescription.Disease.Name,
                DiseaseId = prescription.Disease.Id,
                DiseaseDescription = prescription.Disease.Description,
                Notes = prescription.Notes,
                RecommendDay = prescription.RecommendDay,
                IsDeleted = prescription.IsDeleted,
                StandardPrescriptionMedications = prescription.StandardPrescriptionMedications.Select(spm => new StandardPrescriptionMedicationModel
                {
                    MedicationName = spm.Medication.Name,
                    MedicationId = spm.MedicationId,
                    Id = spm.Id,
                    UsageInstructions= spm.Medication.UsageInstructions,
                    Morning = spm.Morning,
                    Noon = spm.Noon,
                    Afternoon = spm.Afternoon,
                    Evening = spm.Evening
                }).ToList()
            };
        }


    }


}
