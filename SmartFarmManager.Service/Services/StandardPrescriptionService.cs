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

        public async Task<List<StandardPrescriptionModel>> GetStandardPrescriptionsByDiseaseIdAsync(Guid diseaseId)
        {
            // Lấy danh sách StandardPrescription từ database
            var prescriptions = await _unitOfWork.StandardPrescriptions
                .FindByCondition(sp => sp.DiseaseId == diseaseId)
                .Include(sp => sp.StandardPrescriptionMedications)
                .ThenInclude(spm => spm.Medication)
                .ToListAsync();

            // Chuyển đổi sang model
            return prescriptions.Select(sp => new StandardPrescriptionModel
            {
                Id = sp.Id,
                Notes = sp.Notes,
                DiseaseId = sp.DiseaseId,
                Medications = sp.StandardPrescriptionMedications.Select(spm => new StandardPrescriptionMedicationModel
                {
                    Id = spm.Id,
                    MedicationId = spm.MedicationId,
                    MedicationName = spm.Medication.Name,
                    Dosage = spm.Dosage,
                    Morning = spm.Morning,
                    Afternoon = spm.Afternoon,
                    Evening = spm.Evening,
                    Noon = spm.Noon
                }).ToList()
            }).ToList();
        }
    }

}
