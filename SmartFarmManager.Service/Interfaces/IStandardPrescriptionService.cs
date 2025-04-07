using SmartFarmManager.Service.BusinessModels;
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
        Task<StandardPrescriptionModel?> GetStandardPrescriptionsByDiseaseIdAsync(Guid diseaseId);
        Task<bool> CreateStandardPrescriptionAsync(CreateStandardPrescriptionModel model);
        Task<bool> UpdateStandardPrescriptionAsync(Guid id, UpdateStandardPrescriptionModel model);
        Task<bool> DeleteStandardPrescriptionAsync(Guid id);
        Task<PagedResult<StandardPrescriptionItemModel>> GetStandardPrescriptionsAsync(StandardPrescriptionFilterModel filter);
        Task<StandardPrescriptionDetailResponseModel> GetStandardPrescriptionDetailAsync(Guid id);
    }

}
