using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.MedicalSymptom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IMedicalSymptomService
    {
        Task<MedicalSymptomModel?> CreateMedicalSymptomAsync(MedicalSymptomModel medicalSymptom);
        Task<MedicalSymptomModel?> GetMedicalSymptomByIdAsync(Guid id);
        Task<IEnumerable<MedicalSymptomModel>> GetMedicalSymptomsAsync(string? status, DateTime? startDate, DateTime? endDate, string? searchTerm);
        Task<bool> UpdateMedicalSymptomAsync(UpdateMedicalSymptomModel updatedSymptom);
        Task<IEnumerable<MedicalSymptomModel>> GetMedicalSymptomsByStaffAndBatchAsync(Guid? staffId, Guid? farmBatchId);
        System.Threading.Tasks.Task ProcessMedicalSymptomReminderAsync(Guid medicalSymptomId);
    }
}
