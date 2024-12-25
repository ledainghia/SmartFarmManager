using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.Prescription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IPrescriptionService
    {
        Task<Guid> CreatePrescriptionAsync(PrescriptionModel model);
        Task<PrescriptionModel> GetPrescriptionByIdAsync(Guid id);
    }
}
