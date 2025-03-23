using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Vaccine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IVaccineService
    {
        Task<VaccineModel> GetActiveVaccineByCageIdAsync(Guid cageId);
        Task<bool> CreateVaccineAsync(CreateVaccineModel model);
        Task<bool> UpdateVaccineAsync(Guid id, VaccineUpdateModel model);
        Task<bool> DeleteVaccineAsync(Guid id);
        Task<PagedResult<VaccineItemModel>> GetVaccinesAsync(VaccineFilterModel filter);
        Task<VaccineDetailResponseModel?> GetVaccineDetailAsync(Guid id);
    }
}
