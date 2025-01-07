using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities.Date;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.MedicalSymptom;
using SmartFarmManager.Service.BusinessModels.Picture;
using SmartFarmManager.Service.BusinessModels.Prescription;
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
                .FindAll().Where(ms => string.IsNullOrEmpty(status) || ms.Status == status).Include(p => p.Pictures).Include(p => p.FarmingBatch).Include(p => p.Prescriptions).ToListAsync();

            return symptoms.Select(ms => new MedicalSymptomModel
            {
                Id = ms.Id,
                FarmingBatchId = ms.FarmingBatchId,
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
                }).ToList(),
                Prescriptions = ms.Prescriptions.Select(p => new PrescriptionModel
                {
                    Id = p.Id,
                    PrescribedDate = p.PrescribedDate,
                    Status = p.Status,
                    QuantityAnimal = p.QuantityAnimal,
                    Notes = p.Notes,
                    Price = p.Price,
                    DaysToTake = p.DaysToTake,
                    EndDate = p.EndDate,
                }).ToList(),
            });
        }
        public async Task<bool> UpdateMedicalSymptomAsync(UpdateMedicalSymptomModel updatedModel)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingSymptom = await _unitOfWork.MedicalSymptom
                .GetByIdAsync(updatedModel.Id);

                if (existingSymptom == null)
                {
                    return false;
                }

                // Danh sách trạng thái hợp lệ
                var validStatuses = new[]
                {
            MedicalSymptomStatuseEnum.Normal,
            MedicalSymptomStatuseEnum.Diagnosed
        };

                // Kiểm tra trạng thái có hợp lệ không
                if (!validStatuses.Contains(updatedModel.Status))
                {
                    throw new ArgumentException($"Trạng thái không hợp lệ: {updatedModel.Status}");
                }

                // Cập nhật thông tin
                existingSymptom.Diagnosis = updatedModel.Diagnosis;
                existingSymptom.Status = updatedModel.Status;
                existingSymptom.Notes = updatedModel.Notes;
                var cage = await _unitOfWork.Cages.FindByCondition(c => c.IsDeleted == false && c.IsSolationCage == true).FirstOrDefaultAsync();
                // Tạo mới Prescription nếu có
                if (updatedModel.Prescriptions != null)
                {
                    var newPrescription = new Prescription
                    {
                        MedicalSymtomId = updatedModel.Id,
                        CageId = cage.Id,
                        PrescribedDate = updatedModel.Prescriptions.PrescribedDate,
                        Notes = updatedModel.Prescriptions.Notes,
                        DaysToTake = updatedModel.Prescriptions.DaysToTake,
                        Status = updatedModel.Prescriptions.Status,
                        QuantityAnimal = updatedModel.Prescriptions.QuantityAnimal.Value,
                        EndDate = updatedModel.Prescriptions.PrescribedDate.Value.AddDays((double)updatedModel.Prescriptions.DaysToTake)

                    };

                    await _unitOfWork.Prescription.CreateAsync(newPrescription);
                    var newPrescriptionMedication = updatedModel.Prescriptions.Medications.Select(m => new PrescriptionMedication
                    {
                        PrescriptionId = newPrescription.Id,
                        MedicationId = m.MedicationId,
                        Dosage = m.Dosage,
                        Morning = m.Morning,
                        Afternoon = m.Afternoon,
                        Evening = m.Evening,
                        Noon = m.Noon
                    }).ToList();
                    await _unitOfWork.PrescriptionMedications.CreateListAsync(newPrescriptionMedication);
                }
                await _unitOfWork.MedicalSymptom.UpdateAsync(existingSymptom);
                await _unitOfWork.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                Console.WriteLine($"Error in CreateFarmingBatchAsync: {ex.Message}");
                throw new Exception("Failed to create Farming Batch. Details: " + ex.Message);
            }
        }
        public async Task<Guid> CreateMedicalSymptomAsync(MedicalSymptomModel medicalSymptomModel)
        {
            // Bước 1: Tạo đối tượng MedicalSymptom mà chưa có MedicalSymptomDetails và Pictures
            var medicalSymptom = new DataAccessObject.Models.MedicalSymptom
            {
                FarmingBatchId = medicalSymptomModel.FarmingBatchId,
                PrescriptionId = medicalSymptomModel.PrescriptionId,
                Status = MedicalSymptomStatuseEnum.Pending,
                AffectedQuantity = medicalSymptomModel.AffectedQuantity,
                Notes = medicalSymptomModel.Notes,
                CreateAt = DateTimeUtils.VietnamNow()
            };

            // Bước 2: Lưu đối tượng MedicalSymptom vào cơ sở dữ liệu
            await _unitOfWork.MedicalSymptom.CreateAsync(medicalSymptom);
            await _unitOfWork.CommitAsync();

            // Bước 3: Tạo MedicalSymptomDetails và Pictures với MedicalSymptomId
            var medicalSymptomDetails = medicalSymptomModel.MedicalSymptomDetails.Select(d => new DataAccessObject.Models.MedicalSymtomDetail
            {
                SymptomId = d.SymptomId,
                MedicalSymptomId = medicalSymptom.Id, // Gán ID sau khi lưu
            }).ToList();

            var pictures = medicalSymptomModel.Pictures.Select(p => new DataAccessObject.Models.Picture
            {
                RecordId = medicalSymptom.Id, // Gán ID sau khi lưu
                Image = p.Image,
                DateCaptured = p.DateCaptured
            }).ToList();



            // Bước 6: Cập nhật lại MedicalSymptom
            await _unitOfWork.Pictures.CreateListAsync(pictures);
            await _unitOfWork.MedicalSymptomDetails.CreateListAsync(medicalSymptomDetails);
            await _unitOfWork.CommitAsync();

            return medicalSymptom.Id;
        }


        public async Task<MedicalSymptomModel?> GetMedicalSymptomByIdAsync(Guid id)
        {
            var medicalSymptom = await _unitOfWork.MedicalSymptom.FindAll()
                .Where(m => m.Id == id).Include(p => p.Pictures).Include(p => p.FarmingBatch).Include(p => p.Prescriptions).FirstOrDefaultAsync();

            if (medicalSymptom == null)
            {
                return null;
            }

            return new MedicalSymptomModel
            {
                Id = medicalSymptom.Id,
                FarmingBatchId = medicalSymptom.FarmingBatchId,
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
                }).ToList(),
                Prescriptions = medicalSymptom.Prescriptions.Select(p => new PrescriptionModel
                {
                    Id = p.Id,
                    PrescribedDate = p.PrescribedDate,
                    Status = p.Status,
                    QuantityAnimal = p.QuantityAnimal,
                    Notes = p.Notes,
                    Price = p.Price,
                    DaysToTake = p.DaysToTake,
                    EndDate = p.EndDate,
                }).ToList(),
            };
        }
        public async Task<IEnumerable<MedicalSymptomModel>> GetMedicalSymptomsByStaffAndBatchAsync(Guid? staffId, Guid? farmBatchId)
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
