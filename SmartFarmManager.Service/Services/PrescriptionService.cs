using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Medication;
using SmartFarmManager.Service.BusinessModels.Prescription;
using SmartFarmManager.Service.BusinessModels.PrescriptionMedication;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PrescriptionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> CreatePrescriptionAsync(PrescriptionModel model)
        {
            // Lấy danh sách thuốc từ cơ sở dữ liệu
            var medicationIds = model.Medications.Select(m => m.MedicationId).Distinct();
            var medications = await _unitOfWork.Medication
                .FindByCondition(m => medicationIds.Contains(m.Id))
                .ToListAsync();

            if (medications == null || medications.Count == 0)
                throw new Exception("One or more medications not found.");

            // Tính toán giá thuốc
            var totalPrice = model.Medications.Sum(m =>
            {
                var medication = medications.FirstOrDefault(dbMed => dbMed.Id == m.MedicationId);
                if (medication == null)
                    throw new Exception($"Medication with ID {m.MedicationId} not found.");

                return (decimal)(m.Dosage * (medication.PricePerDose ?? 0));
            });
            var prescription = new Prescription
            {
                RecordId = model.RecordId,
                PrescribedDate = model.PrescribedDate,
                Notes = model.Notes,
                QuantityAnimal = model.QuantityAnimal,
                Status = model.Status,
                Price = totalPrice,
                PrescriptionMedications = model.Medications.Select(m => new PrescriptionMedication
                {
                    MedicationId = m.MedicationId,
                    Dosage = m.Dosage,
                    Morning = m.Morning,
                    Afternoon = m.Afternoon,
                    Evening = m.Evening,
                    Night = m.Night,
                }).ToList()
            };

            await _unitOfWork.Prescription.CreateAsync(prescription);
            await _unitOfWork.CommitAsync();

            return prescription.Id;
        }

        public async Task<PrescriptionModel> GetPrescriptionByIdAsync(Guid id)
        {
            var prescription = await _unitOfWork.Prescription
                .FindByCondition(p => p.Id == id, true, p => p.PrescriptionMedications, p => p.PrescriptionMedications.Select(pm => pm.Medication))
                .FirstOrDefaultAsync();

            if (prescription == null)
                return null;

            return new PrescriptionModel
            {
                Id = prescription.Id,
                RecordId = prescription.RecordId,
                PrescribedDate = prescription.PrescribedDate.Value,
                Notes = prescription.Notes,
                QuantityAnimal = prescription.QuantityAnimal,
                Status = prescription.Status,
                Price = prescription.Price,
                Medications = prescription.PrescriptionMedications.Select(pm => new PrescriptionMedicationModel
                {
                    MedicationId = pm.MedicationId,
                    Dosage = pm.Dosage.Value,
                    Morning = pm.Morning,
                    Afternoon = pm.Afternoon,
                    Evening = pm.Evening,
                    Night = pm.Night,
                    Medication = new MedicationModel
                    {
                        Name = pm.Medication.Name,
                        UsageInstructions = pm.Medication.UsageInstructions,
                        Price = pm.Medication.Price,
                        DoseQuantity = pm.Medication.DoseQuantity
                    }
                }).ToList()
            };
        }
    }
}
