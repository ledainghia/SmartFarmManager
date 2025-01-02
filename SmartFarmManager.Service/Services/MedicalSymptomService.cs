using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.MedicalSymptom;
using SmartFarmManager.Service.BusinessModels.Picture;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class MedicalSymptomService : IMedicalSymptomService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicalSymptomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MedicalSymptomModel>> GetMedicalSymptomsAsync(string? status)
        {
            var symptoms = await _unitOfWork.MedicalSymptom
                .FindAll().Where(ms => string.IsNullOrEmpty(status) || ms.Status == status).Include(p => p.Pictures).Include(p => p.FarmingBatch).ToListAsync();

            return symptoms.Select(ms => new MedicalSymptomModel
            {
                Id = ms.Id,
                FarmingBatchId = ms.FarmingBatchId,
                Symptoms = ms.Symptoms,
                Diagnosis = ms.Diagnosis,
                Status = ms.Status,
                AffectedQuantity = ms.AffectedQuantity,
                Notes = ms.Notes,
                Quantity = ms.FarmingBatch?.Quantity ?? 0,
                NameAnimal = ms.FarmingBatch.Species,
                CreateAt = ms.CreateAt, 
                Pictures = ms.Pictures.Select(p => new PictureModel
                {
                    Id = p.Id,
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList()
            });
        }
        public async Task<bool> UpdateMedicalSymptomAsync(MedicalSymptomModel updatedModel)
        {
            var existingSymptom = await _unitOfWork.MedicalSymptom
                .GetByIdAsync(updatedModel.Id);

            if (existingSymptom == null)
            {
                return false;
            }

            // Cập nhật thông tin
            existingSymptom.Diagnosis = updatedModel.Diagnosis;
            existingSymptom.Status = updatedModel.Status;
            existingSymptom.Notes = updatedModel.Notes;

            await _unitOfWork.MedicalSymptom.UpdateAsync(existingSymptom);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<Guid> CreateMedicalSymptomAsync(MedicalSymptomModel medicalSymptomModel)
        {
            var medicalSymptom = new DataAccessObject.Models.MedicalSymptom
            {
                FarmingBatchId = medicalSymptomModel.FarmingBatchId,
                Symptoms = medicalSymptomModel.Symptoms,
                Status = medicalSymptomModel.Status,
                AffectedQuantity = medicalSymptomModel.AffectedQuantity,
                Notes = medicalSymptomModel.Notes,
                CreateAt = DateTime.UtcNow,
                Pictures = medicalSymptomModel.Pictures.Select(p => new DataAccessObject.Models.Picture
                {
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList()
            };

            await _unitOfWork.MedicalSymptom.CreateAsync(medicalSymptom);
            await _unitOfWork.CommitAsync();

            return medicalSymptom.Id;
        }

        public async Task<MedicalSymptomModel?> GetMedicalSymptomByIdAsync(Guid id)
        {
            var medicalSymptom = await _unitOfWork.MedicalSymptom.FindAll()
                .Where(m => m.Id == id).Include(p => p.Pictures).Include(p => p.FarmingBatch).FirstOrDefaultAsync();

            if (medicalSymptom == null)
            {
                return null;
            }

            return new MedicalSymptomModel
            {
                Id = medicalSymptom.Id,
                FarmingBatchId = medicalSymptom.FarmingBatchId,
                Symptoms = medicalSymptom.Symptoms,
                Diagnosis = medicalSymptom.Diagnosis,
                Status = medicalSymptom.Status,
                AffectedQuantity = medicalSymptom.AffectedQuantity,
                Notes = medicalSymptom.Notes,
                Quantity = medicalSymptom.FarmingBatch.Quantity,
                NameAnimal = medicalSymptom.FarmingBatch.Species,
                CreateAt = medicalSymptom.CreateAt,
                Pictures = medicalSymptom.Pictures.Select(p => new PictureModel
                {
                    Id = p.Id,
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList()
            };
        }
        public async Task<IEnumerable<MedicalSymptomModel>> GetMedicalSymptomsByStaffAndBatchAsync(Guid staffId, Guid farmBatchId)
        {
            // Lấy danh sách CageStaff theo Staff ID
            var cageStaffs = await _unitOfWork.CageStaffs
                .FindByCondition(cs => cs.StaffFarmId == staffId, trackChanges: false, cs => cs.Cage)
                .ToListAsync();

            if (!cageStaffs.Any())
            {
                return Enumerable.Empty<MedicalSymptomModel>();
            }

            // Lấy danh sách Cage IDs từ CageStaff
            var cageIds = cageStaffs.Select(cs => cs.CageId).Distinct();

            // Kiểm tra xem FarmBatchId có thuộc Cage của Staff không
            var farmingBatch = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => cageIds.Contains(fb.CageId) && fb.Id == farmBatchId, trackChanges: false, fb => fb.MedicalSymptoms)
                .FirstOrDefaultAsync();

            if (farmingBatch == null)
            {
                return Enumerable.Empty<MedicalSymptomModel>();
            }

            // Lấy danh sách Medical Symptoms từ Farming Batch
            return farmingBatch.MedicalSymptoms.Select(ms => new MedicalSymptomModel
            {
                Id = ms.Id,
                FarmingBatchId = ms.FarmingBatchId,
                Symptoms = ms.Symptoms,
                Diagnosis = ms.Diagnosis,
                Status = ms.Status,
                AffectedQuantity = ms.AffectedQuantity,
                Quantity = farmingBatch.Quantity,
                Notes = ms.Notes,
                CreateAt = ms.CreateAt,
            });
        }
    }
}
