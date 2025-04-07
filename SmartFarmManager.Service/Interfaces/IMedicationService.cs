using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Medication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IMedicationService
    {
        Task<bool> UpdateMedicationAsync(Guid id, UpdateMedicationModel model);
        Task<bool> DeleteMedicationAsync(Guid id);
        Task<MedicationDetailResponseModel?> GetMedicationDetailAsync(Guid id);
        Task<MedicationModel?> CreateMedicationAsync(MedicationModel medication);
        Task<IEnumerable<MedicationModel>> GetAllMedicationsAsync();
        Task<MedicationModel?> GetMedicationByName(string name);
        Task<PagedResult<MedicationModel>> GetMedicationsAsync(MedicationFilterModel filter);
    }

}
