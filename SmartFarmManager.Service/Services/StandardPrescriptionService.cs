using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.StandardPrescription;
using SmartFarmManager.Service.BusinessModels.StandardPrescriptionMedication;
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
    }

}
