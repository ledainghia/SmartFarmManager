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
        IStatusRepository Statuses { get; }
        IStatusLogRepository StatusLogs { get; }
        ICageRepository Cages { get; }
        ICageStaffRepository CageStaffs { get; }
        IMedicalSymptomRepository MedicalSymptom { get; }
        IPrescriptionRepository Prescription { get; }
        IMedicationRepository Medication { get; }
        IFarmingBatchRepository FarmingBatch { get; }
        IRoleRepository Roles { get; }
        IFarmRepository Farms { get; }
        IFarmAdminRepository FarmsAdmins { get; }
        IAnimalTemplateRepository AnimalTemplates { get; }

        Task<int> CommitAsync();
        System.Threading.Tasks.Task BeginTransactionAsync();
        System.Threading.Tasks.Task RollbackAsync();
    }
}
