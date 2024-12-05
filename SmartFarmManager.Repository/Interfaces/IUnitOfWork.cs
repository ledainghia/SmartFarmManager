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
        
        Task<int> CommitAsync();
        System.Threading.Tasks.Task BeginTransactionAsync();
        System.Threading.Tasks.Task RollbackAsync();
    }
}
