using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Dashboard;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class FarmDashboardService:IFarmDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FarmDashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<DashboardStatisticsModel> GetFarmDashboardStatisticsAsync(Guid farmId, DateTime? startDate, DateTime? endDate)
        {
            var dashboardModel = new DashboardStatisticsModel();

            // Task Statistics
            dashboardModel.TaskStatistics = await GetTaskStatisticsAsync(farmId, startDate, endDate);

            // Cage Statistics
            dashboardModel.CageStatistics = await GetCageStatisticsAsync(farmId);

            // FarmingBatch Statistics
            dashboardModel.FarmingBatchStatistics = await GetFarmingBatchStatisticsAsync(farmId, startDate, endDate);

            // Staff Statistics
            dashboardModel.StaffStatistics = await GetStaffStatisticsAsync(farmId);

            // VaccineSchedule Statistics
            dashboardModel.VaccineScheduleStatistics = await GetVaccineScheduleStatisticsAsync(farmId, startDate, endDate);

            return dashboardModel;
        }

        private async Task<TaskStatisticModel> GetTaskStatisticsAsync(Guid farmId, DateTime? startDate, DateTime? endDate)
        {
            var query = _unitOfWork.Tasks
                .FindByCondition(t => t.Cage.FarmId == farmId)
                .Include(x => x.Cage)
                .AsQueryable();

            // Filter theo khoảng thời gian nếu có
            if (startDate.HasValue)
            {
                query = query.Where(t => t.DueDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.DueDate <= endDate.Value);
            }

            var taskStats = new TaskStatisticModel
            {
                TotalTasks = await query.CountAsync(),
                PendingTasks = await query.Where(t => t.Status == TaskStatusEnum.Pending).CountAsync(),
                InProgressTasks = await query.Where(t => t.Status == TaskStatusEnum.InProgress).CountAsync(),
                DoneTasks = await query.Where(t => t.Status == TaskStatusEnum.Done).CountAsync(),
                CancelledTasks = await query.Where(t => t.Status == TaskStatusEnum.Cancelled).CountAsync(),
                OverdueTasks = await query.Where(t => t.Status == TaskStatusEnum.Overdue).CountAsync(),
            };

            return taskStats;
        }



        private async Task<CageStatisticModel> GetCageStatisticsAsync(Guid farmId)
        {
            var query = _unitOfWork.Cages
                .FindByCondition(c => c.FarmId == farmId)
                .Include(c => c.FarmingBatches)
                .AsQueryable(); // Khởi tạo query ban đầu

            // Tính toán các thống kê
            var cageStats = new CageStatisticModel
            {
                // Tổng số chuồng trong farm
                TotalCages = await query.CountAsync(),

                NormalCages = await query
                    .Where(c => c.IsSolationCage == false)
                    .CountAsync(),
                IsolatedCages = await query
                    .Where(c => c.IsSolationCage == true)
                    .CountAsync(),
                // Tổng số chuồng trống (không có FarmingBatch nào)
                EmptyCages = await query
                    .Where(c => !c.FarmingBatches.Any(fb => fb.Status == FarmingBatchStatusEnum.Active))
                    .CountAsync(),

                // Tổng số chuồng có FarmingBatch đang hoạt động
                CagesWithActiveFarmingBatches = await query
                    .Where(c => c.FarmingBatches.Any(fb => fb.Status == FarmingBatchStatusEnum.Active))
                    .CountAsync()
            };

            return cageStats;
        }


        private async Task<FarmingBatchStatisticModel> GetFarmingBatchStatisticsAsync(Guid farmId, DateTime? startDate, DateTime? endDate)
        {
            var query = _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.Cage.FarmId == farmId)
                .Include(fb => fb.Cage)
                .AsQueryable();

            // Filter theo khoảng thời gian nếu có
            if (startDate.HasValue)
            {
                query = query.Where(fb => fb.EstimatedTimeStart >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(fb => fb.EstimatedTimeStart <= endDate.Value);
            }

            var farmingBatchStats = new FarmingBatchStatisticModel
            {
                TotalFarmingBatches = await query.CountAsync(),
                PlanningFarmingBatches = await query.Where(fb => fb.Status == FarmingBatchStatusEnum.Planning).CountAsync(),
                ActiveFarmingBatches = await query.Where(fb => fb.Status == FarmingBatchStatusEnum.Active).CountAsync(),
                CompletedFarmingBatches = await query.Where(fb => fb.Status == FarmingBatchStatusEnum.Completed).CountAsync(),
                CancelledFarmingBatches = await query.Where(fb => fb.Status == FarmingBatchStatusEnum.Cancelled).CountAsync()
            };

            return farmingBatchStats;
        }


        private async Task<StaffStatisticModel> GetStaffStatisticsAsync(Guid farmId)
        {
            var query = _unitOfWork.Users
       .FindByCondition(u =>  u.Role.RoleName == "Staff Farm")
       .Include(u => u.Role)
       .AsQueryable();
            var staffStats = new StaffStatisticModel
            {
                TotalStaff = await query.CountAsync(),
            };

            return staffStats;
        }

        private async Task<VaccineScheduleStatisticModel> GetVaccineScheduleStatisticsAsync(Guid farmId, DateTime? startDate, DateTime? endDate)
        {
            var query = _unitOfWork.VaccineSchedules
                .FindByCondition(vs => vs.Stage.FarmingBatch.Cage.FarmId == farmId)
                .Include(vs => vs.Stage)
                .ThenInclude(s => s.FarmingBatch)
                .ThenInclude(fb => fb.Cage)
                .AsQueryable();

            // Filter theo khoảng thời gian nếu có
            if (startDate.HasValue)
            {
                query = query.Where(vs => vs.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(vs => vs.Date <= endDate.Value);
            }

            var vaccineScheduleStats = new VaccineScheduleStatisticModel
            {
                TotalVaccineSchedules = await query.CountAsync(),
                UpcomingVaccineSchedules = await query.Where(vs => vs.Status == VaccineScheduleStatusEnum.Upcoming).CountAsync(),
                CompletedVaccineSchedules = await query.Where(vs => vs.Status == VaccineScheduleStatusEnum.Completed).CountAsync(),
                MissedVaccineSchedules = await query.Where(vs => vs.Status == VaccineScheduleStatusEnum.Missed).CountAsync(),
                CancelledVacineSchedules = await query.Where(vs => vs.Status == VaccineScheduleStatusEnum.Cancelled).CountAsync(),
                RedoVaccineSchedules = await query.Where(vs => vs.Status == VaccineScheduleStatusEnum.Redo).CountAsync()
            };

            return vaccineScheduleStats;
        }


    }
}
