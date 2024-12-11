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
        Task<Guid> CreateMedicalSymptomAsync(MedicalSymptomModel medicalSymptom);
        Task<MedicalSymptomModel?> GetMedicalSymptomByIdAsync(Guid id);
        Task<IEnumerable<MedicalSymptomModel>> GetMedicalSymptomsAsync(string? status);
        Task<bool> UpdateMedicalSymptomAsync(MedicalSymptomModel updatedSymptom);
    }
}
