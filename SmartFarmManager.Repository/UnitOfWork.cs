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
        public IStatusRepository Statuses { get; }
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
        public ITemporaryCageAssignmentRepository TemporaryCageAssignments { get; }
        public IVaccineScheduleRepository VaccineSchedules { get; }

        public UnitOfWork(SmartFarmContext context, IUserRepository users,
            ITaskTypeRepository taskTypes,
            ITaskRepository tasks,
            IStatusRepository statuses,
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
            ITemporaryCageAssignmentRepository temporaryCageAssignments,
            IVaccineScheduleRepository vaccineSchedules)
        {
            _context = context;
            Users = users;
            TaskTypes = taskTypes;
            Tasks = tasks;
            Statuses = statuses;
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
            TemporaryCageAssignments= temporaryCageAssignments;
            VaccineSchedules= vaccineSchedules;
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
