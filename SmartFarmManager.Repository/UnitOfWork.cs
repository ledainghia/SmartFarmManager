using Microsoft.EntityFrameworkCore.Storage;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SmartFarmContext _context;
        private IDbContextTransaction _currentTransaction;

        public IUserRepository Users { get;}
        public ITaskRepository Tasks { get;}
        public ITaskTypeRepository TaskTypes { get;}
        public IStatusLogRepository StatusLogs { get; }
        public ICageRepository Cages { get; }
        public ICageStaffRepository CageStaffs { get; }
        public IMedicalSymptomRepository MedicalSymptom { get; }
        public IPrescriptionRepository Prescription { get; }
        public IMedicationRepository Medication { get; }
        public IFarmingBatchRepository FarmingBatches { get; }
        public IRoleRepository Roles { get; }
        public IFarmRepository Farms { get; }
        public IFarmAdminRepository FarmsAdmins { get; }
        public IAnimalTemplateRepository AnimalTemplates { get; }
        public IGrowthStageTemplateRepository GrowthStageTemplates { get; }
        public ITaskDailyTemplateRepository TaskDailyTemplates { get; }
        public IFoodTemplateRepository FoodTemplates { get; }
        public IVaccineTemplateRepository VaccineTemplates { get; }
        public IGrowthStageRepository GrowthStages { get; }
        public IVaccineRepository Vaccines { get; }
        public ITaskDailyRepository TaskDailies { get; }
        public IHealthLogRepository HealthLogs { get; }
        public IPictureRepository Pictures { get; }
        public IPrescriptionMedicationRepository PrescriptionMedications { get; }
        public IVaccineScheduleLogRepository VaccineScheduleLogs { get; }
        public IDailyFoodUsageLogRepository DailyFoodUsageLogs { get; }
        public IVaccineScheduleRepository VaccineSchedules { get; }
        public ILeaveRequestRepository LeaveRequests { get; }
        public ISaleTypeRepository SaleTypes { get; }
        public IMedicalSymptomDetailRepository MedicalSymptomDetails { get; }
        public ISymptomRepository Symptoms { get; }
        public IDiseaseRepositoy Diseases { get; }
        public IStandardPrescriptionRepository StandardPrescriptions { get; }
        public IFoodStackRepository FoodStacks { get; }
        public INotificationRepository Notifications { get; }
        public INotificationTypeRepository NotificationsTypes { get; }
        public ICostingReportsRepository CostingReports { get; }
        public IElectricityLogsRepository ElectricityLogs { get; }
        public IWaterLogsRepository WaterLogs { get; }

        public IAnimalSalesRepository AnimalSales { get; }

        public IMasterDataRepository MasterData { get; }
        public IWhiteListDomainRepository WhiteListDomains{ get; }
        public ISensorRepository Sensors { get; }
        public ISensorTypeRepository SensorTypes { get; }
        public ISensorDataLogRepository SensorDataLogs { get; }
        public IStockLogRepository StockLogs { get; }
        public IEggHarvestRepository EggHarvests { get; }
        public IFarmConfigRepository FarmConfigs { get; }
        public IStandardPrescriptionMedicationRepository StandardPrescriptionMedications { get; }

        public UnitOfWork(SmartFarmContext context, IUserRepository users,
            ITaskTypeRepository taskTypes,
            ITaskRepository tasks,
            IStatusLogRepository statusLogs,
            ICageRepository cages,
            ICageStaffRepository cageStaffs,
            IMedicationRepository medications,
            IMedicalSymptomRepository medicalSymptoms,
            IPrescriptionRepository prescriptions,
            IFarmingBatchRepository farmingBatchs,
            IRoleRepository roles, 
            IFarmRepository farms, 
            IFarmAdminRepository farmAdmins,
            IAnimalTemplateRepository animalTemplates,
            IGrowthStageTemplateRepository growthStageTemplates,
            ITaskDailyTemplateRepository taskDailyTemplates,
            IFoodTemplateRepository foodTemplates,
            IGrowthStageRepository growthStages,
            IVaccineTemplateRepository vaccineTemplates,
            IVaccineRepository vaccines,
            IHealthLogRepository healthLogs,
            IPictureRepository pictures,
            IPrescriptionMedicationRepository prescriptionMedications,
            IDailyFoodUsageLogRepository dailyFoodUsageLogs,
            IVaccineScheduleLogRepository vaccineScheduleLogs,
            IVaccineScheduleRepository vaccineSchedules,
            ITaskDailyRepository taskDailies,
            ILeaveRequestRepository leaveRequests,
            ISaleTypeRepository saleTypes,
            IMedicalSymptomDetailRepository medicalSymptomDetails,
            ISymptomRepository symptoms,
            IDiseaseRepositoy diseases,
            IStandardPrescriptionRepository standardPrescriptions,
            IFoodStackRepository foodStacks,
            INotificationRepository notifications,
            INotificationTypeRepository notificationTypes,
            ICostingReportsRepository costingReports,
            IElectricityLogsRepository electricityLogs,
            IWaterLogsRepository waterLogs,
            IAnimalSalesRepository animalSales,
            IMasterDataRepository masterData,
            IWhiteListDomainRepository whiteListDomains,
            ISensorRepository sensors,
            ISensorTypeRepository sensorTypes,
            ISensorDataLogRepository sensorDataLogs,
            IStockLogRepository stockLogs,
            IEggHarvestRepository eggHarvests,
            IFarmConfigRepository farmConfigs,
            IStandardPrescriptionMedicationRepository standardPrescriptionMedications)
        {
            _context = context;
            Users = users;
            TaskTypes = taskTypes;
            Tasks = tasks;
            StatusLogs = statusLogs;
            Cages = cages;
            CageStaffs = cageStaffs;
            Medication = medications;
            MedicalSymptom = medicalSymptoms;
            Prescription = prescriptions;
            FarmingBatches = farmingBatchs;
            Roles = roles;
            Farms = farms;
            FarmsAdmins = farmAdmins;
            AnimalTemplates = animalTemplates;
            GrowthStageTemplates = growthStageTemplates;
            TaskDailyTemplates = taskDailyTemplates;
            FoodTemplates = foodTemplates;
            GrowthStages = growthStages;
            VaccineTemplates = vaccineTemplates;
            Vaccines= vaccines;
            VaccineSchedules= vaccineSchedules;
            HealthLogs = healthLogs;
            Pictures = pictures;
            PrescriptionMedications = prescriptionMedications;
            DailyFoodUsageLogs = dailyFoodUsageLogs;
            VaccineScheduleLogs = vaccineScheduleLogs;
            VaccineSchedules = vaccineSchedules;
            TaskDailies = taskDailies;
            LeaveRequests=leaveRequests;
            SaleTypes = saleTypes;
            MedicalSymptomDetails = medicalSymptomDetails;
            Symptoms = symptoms;
            Diseases = diseases;
            StandardPrescriptions = standardPrescriptions;
            FoodStacks = foodStacks;
            Notifications = notifications;
            NotificationsTypes = notificationTypes;
            CostingReports = costingReports;
            ElectricityLogs = electricityLogs;
            WaterLogs = waterLogs;
            AnimalSales = animalSales;
            MasterData = masterData;
            WhiteListDomains = whiteListDomains;
            Sensors = sensors;
            SensorTypes = sensorTypes;
            SensorDataLogs = sensorDataLogs;
            StockLogs = stockLogs;
            EggHarvests = eggHarvests;
            FarmConfigs = farmConfigs;
            StandardPrescriptionMedications = standardPrescriptionMedications;
        }
         

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }

        public async System.Threading.Tasks.Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task<int> CommitAsync()
        {
            try
            {
                var result = await _context.SaveChangesAsync();

                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync();
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }

                return result;
            }
            catch
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync();
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }

                throw;
            }
        }

        public async System.Threading.Tasks.Task RollbackAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public SmartFarmContext GetDbContext()
        {
            return _context;
        }
    }
}
