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
    }
}
