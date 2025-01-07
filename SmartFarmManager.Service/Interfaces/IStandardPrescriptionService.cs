using SmartFarmManager.Service.BusinessModels.StandardPrescription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IStandardPrescriptionService
    {
        Task<List<StandardPrescriptionModel>> GetStandardPrescriptionsByDiseaseIdAsync(Guid diseaseId);
    }

}
