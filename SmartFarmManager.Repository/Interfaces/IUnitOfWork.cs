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

        // Add other repository properties as needed
        IAlertRepository Alerts { get; }
        IAlertTypeRepository AlertTypes { get; }
        IAlertUserRepository AlertUsers { get; }
        ICameraSurveillanceRepository CameraSurveillances { get; }
        IDeviceReadingRepository DeviceReadings { get; }
        IFarmRepository Farms { get; }
        IFarmStaffAssignmentRepository FarmStaffAssignments { get; }
        IInventoryRepository Inventories { get; }
        IInventoryTransactionRepository InventoryTransactions { get; }
        IIoTDeviceRepository IoTDevices { get; }
        ILivestockRepository Livestocks { get; }
        ILivestockExpenseRepository LivestockExpenses { get; }
        ILivestockSaleRepository LivestockSales { get; }
        INotificationRepository Notifications { get; }
        IPermissionRepository Permissions { get; }
        IRevenueAndProfitReportRepository RevenueAndProfitReports { get; }
        IRoleRepository Roles { get; }
        ITaskRepository Tasks { get; }
        ITaskHistoryRepository TaskHistories { get; }
        IUserPermissionRepository UserPermissions { get; }

        FarmsContext GetDbContext();

        Task<int> CommitAsync();
        System.Threading.Tasks.Task BeginTransactionAsync();
        System.Threading.Tasks.Task RollbackAsync();
    }
}
