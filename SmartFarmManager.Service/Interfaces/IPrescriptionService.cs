﻿using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.MedicalSymptom;
using SmartFarmManager.Service.BusinessModels.Prescription;
using SmartFarmManager.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IPrescriptionService
    {
        Task<Guid?> CreatePrescriptionAsync(PrescriptionModel model);
        Task<PrescriptionModel> GetPrescriptionByIdAsync(Guid id);
        Task<IEnumerable<PrescriptionModel>> GetActivePrescriptionsByCageIdAsync(Guid cageId);
        Task<bool> UpdatePrescriptionAsync(PrescriptionModel model);

        Task<bool> IsLastPrescriptionSessionAsync(Guid prescriptionId);
        Task<bool> UpdatePrescriptionStatusAsync(Guid prescriptionId, UpdatePrescriptionModel request);
        Task<bool> CreateNewPrescriptionAsync(UpdateMedicalSymptomModel request);
        Task<PagedResult<PrescriptionList>> GetPrescriptionsAsync(
    DateTime? startDate, DateTime? endDate, string? status, string? cageName, int pageNumber, int pageSize);
        Task<List<PrescriptionModel>> GetPrescriptionsHistoryAsync(Guid medicalSymptomId);
        }
}
