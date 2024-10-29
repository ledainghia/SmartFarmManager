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
        private readonly FarmsContext _context;
        private IDbContextTransaction _currentTransaction;

        public IUserRepository Users { get; }
        public IAlertRepository Alerts { get; }
        public IAlertTypeRepository AlertTypes { get; }
        public IAlertUserRepository AlertUsers { get; }
        public ICameraSurveillanceRepository CameraSurveillances { get; }
        public IDeviceReadingRepository DeviceReadings { get; }
        public IFarmRepository Farms { get; }
        public IFarmStaffAssignmentRepository FarmStaffAssignments { get; }
        public IInventoryRepository Inventories { get; }
        public IInventoryTransactionRepository InventoryTransactions { get; }
        public IIoTDeviceRepository IoTDevices { get; }
        public ILivestockRepository Livestocks { get; }
        public ILivestockExpenseRepository LivestockExpenses { get; }
        public ILivestockSaleRepository LivestockSales { get; }
        public INotificationRepository Notifications { get; }
        public IPermissionRepository Permissions { get; }
        public IRevenueAndProfitReportRepository RevenueAndProfitReports { get; }
        public IRoleRepository Roles { get; }
        public ITaskRepository Tasks { get; }
        public ITaskHistoryRepository TaskHistories { get; }
        public IUserPermissionRepository UserPermissions { get; }

        public UnitOfWork(
            FarmsContext context,
            IUserRepository users,
            IAlertRepository alerts,
            IAlertTypeRepository alertTypes,
            IAlertUserRepository alertUsers,
            ICameraSurveillanceRepository cameraSurveillances,
            IDeviceReadingRepository deviceReadings,
            IFarmRepository farms,
            IFarmStaffAssignmentRepository farmStaffAssignments,
            IInventoryRepository inventories,
            IInventoryTransactionRepository inventoryTransactions,
            IIoTDeviceRepository ioTDevices,
            ILivestockRepository livestocks,
            ILivestockExpenseRepository livestockExpenses,
            ILivestockSaleRepository livestockSales,
            INotificationRepository notifications,
            IPermissionRepository permissions,
            IRevenueAndProfitReportRepository revenueAndProfitReports,
            IRoleRepository roles,
            ITaskRepository tasks,
            ITaskHistoryRepository taskHistories,
            IUserPermissionRepository userPermissions)
        {
            _context = context;
            Users = users;
            Alerts = alerts;
            AlertTypes = alertTypes;
            AlertUsers = alertUsers;
            CameraSurveillances = cameraSurveillances;
            DeviceReadings = deviceReadings;
            Farms = farms;
            FarmStaffAssignments = farmStaffAssignments;
            Inventories = inventories;
            InventoryTransactions = inventoryTransactions;
            IoTDevices = ioTDevices;
            Livestocks = livestocks;
            LivestockExpenses = livestockExpenses;
            LivestockSales = livestockSales;
            Notifications = notifications;
            Permissions = permissions;
            RevenueAndProfitReports = revenueAndProfitReports;
            Roles = roles;
            Tasks = tasks;
            TaskHistories = taskHistories;
            UserPermissions = userPermissions;
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

        public FarmsContext GetDbContext()
        {
            return _context;
        }
    }
}
