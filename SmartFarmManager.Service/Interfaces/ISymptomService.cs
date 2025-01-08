using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Symptom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface ISymptomService
    {
        Task<List<SymptomModel>> GetAllSymptomsAsync();
        Task<SymptomModel> GetSymptomByIdAsync(Guid id);
        Task<Guid> CreateSymptomAsync(SymptomModel symptomModel);
        Task<bool> UpdateSymptomAsync(SymptomModel symptomModel);
        Task<bool> DeleteSymptomAsync(Guid id);
        Task<PagedResult<SymptomModel>> GetPagedSymptomsAsync(string? name, int page, int pageSize);
    }
}
