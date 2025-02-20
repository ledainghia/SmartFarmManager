using SmartFarmManager.DataAccessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ITaskRepository Tasks { get; }
        ITaskTypeRepository TaskTypes { get; }
        IStatusLogRepository StatusLogs { get; }
        ICageRepository Cages { get; }
        ICageStaffRepository CageStaffs { get; }
        IMedicalSymptomRepository MedicalSymptom { get; }
        IPrescriptionRepository Prescription { get; }
        IMedicationRepository Medication { get; }
        IFarmingBatchRepository FarmingBatches { get; }
        IRoleRepository Roles { get; }
        IFarmRepository Farms { get; }
        IFarmAdminRepository FarmsAdmins { get; }
        ITaskDailyTemplateRepository TaskDailyTemplates { get; }
        IGrowthStageTemplateRepository GrowthStageTemplates { get; }
        IAnimalTemplateRepository AnimalTemplates { get; }
        IFoodTemplateRepository FoodTemplates { get; }
        IVaccineTemplateRepository VaccineTemplates { get; }
        IGrowthStageRepository GrowthStages { get; }
        IVaccineRepository Vaccines { get; }
        ITaskDailyRepository TaskDailies { get; }
        IVaccineScheduleRepository VaccineSchedules { get; }
        IHealthLogRepository HealthLogs { get; }
        IPictureRepository Pictures { get; }
        IPrescriptionMedicationRepository PrescriptionMedications { get; }
        IDailyFoodUsageLogRepository DailyFoodUsageLogs { get; }
        IVaccineScheduleLogRepository VaccineScheduleLogs { get; }
        ILeaveRequestRepository LeaveRequests { get; }
        ISaleTypeRepository SaleTypes { get; }
        IMedicalSymptomDetailRepository MedicalSymptomDetails { get; }
        ISymptomRepository Symptoms { get; }
        IDiseaseRepositoy Diseases { get; }
        IStandardPrescriptionRepository StandardPrescriptions { get; }
        IFoodStackRepository FoodStacks { get; }
        INotificationRepository Notifications { get; }
        INotificationTypeRepository NotificationsTypes { get; }
        ICostingReportsRepository CostingReports { get; }
        IElectricityLogsRepository ElectricityLogs { get; }
        IWaterLogsRepository WaterLogs { get; }
        IAnimalSalesRepository AnimalSales { get; }
        IMasterDataRepository MasterData { get; }
        Task<int> CommitAsync();
        System.Threading.Tasks.Task BeginTransactionAsync();
        System.Threading.Tasks.Task RollbackAsync();
    }
}
