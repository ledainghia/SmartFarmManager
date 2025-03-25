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


        public async Task<DashboardStatisticsModel> GetFarmDashboardStatisticsAsync(Guid farmId)
        {
            var dashboardModel = new DashboardStatisticsModel();

            // Task Statistics
            dashboardModel.TaskStatistics = await GetTaskStatisticsAsync(farmId);

            // Cage Statistics
            dashboardModel.CageStatistics = await GetCageStatisticsAsync(farmId);

            // FarmingBatch Statistics
            dashboardModel.FarmingBatchStatistics = await GetFarmingBatchStatisticsAsync(farmId);

            // Staff Statistics
            dashboardModel.StaffStatistics = await GetStaffStatisticsAsync(farmId);

            // VaccineSchedule Statistics
            dashboardModel.VaccineScheduleStatistics = await GetVaccineScheduleStatisticsAsync(farmId);

            return dashboardModel;
        }

        private async Task<TaskStatisticModel> GetTaskStatisticsAsync(Guid farmId)
        {
            var query = _unitOfWork.Tasks
                .FindByCondition(t => t.Cage.FarmId == farmId)
                .Include(x => x.Cage)
                .AsQueryable(); // Khởi tạo query ban đầu

            // Tính toán các thống kê
            var taskStats = new TaskStatisticModel
            {
                TotalTasks = await query.CountAsync(),

                // Các công việc có trạng thái "Pending"
                PendingTasks = await query
                    .Where(t => t.Status == TaskStatusEnum.Pending)
                    .CountAsync(),

                // Các công việc có trạng thái "InProgress"
                InProgressTasks = await query
                    .Where(t => t.Status == TaskStatusEnum.InProgress)
                    .CountAsync(),

                // Các công việc có trạng thái "Completed"
                DoneTasks = await query
                    .Where(t => t.Status == TaskStatusEnum.Done)
                    .CountAsync(),

                // Các công việc bị "Cancelled"
                CancelledTasks = await query
                    .Where(t => t.Status == TaskStatusEnum.Cancelled)
                    .CountAsync(),
                OverdueTasks = await query
                    .Where(t => t.Status == TaskStatusEnum.Overdue)
                    .CountAsync(),
                 
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


        private async Task<FarmingBatchStatisticModel> GetFarmingBatchStatisticsAsync(Guid farmId)
        {
            var query = _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.Cage.FarmId == farmId).Include(fb => fb.Cage)
                .AsQueryable();

            var farmingBatchStats = new FarmingBatchStatisticModel
            {
                TotalFarmingBatches = await query.CountAsync(),
                PlanningFarmingBatches = await query
                    .Where(fb => fb.Status ==  FarmingBatchStatusEnum.Planning)
                    .CountAsync(),
                ActiveFarmingBatches = await query
                    .Where(fb => fb.Status == FarmingBatchStatusEnum.Active)
                    .CountAsync(),
                CompletedFarmingBatches = await query
                    .Where(fb => fb.Status == FarmingBatchStatusEnum.Completed)
                    .CountAsync(),
                CancelledFarmingBatches = await query
                    .Where(fb => fb.Status == FarmingBatchStatusEnum.Cancelled)
                    .CountAsync()
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

        private async Task<VaccineScheduleStatisticModel> GetVaccineScheduleStatisticsAsync(Guid farmId)
        {
            // Truy vấn tất cả VaccineSchedules trong farm
            var query = _unitOfWork.VaccineSchedules
                .FindByCondition(vs => vs.Stage.FarmingBatch.Cage.FarmId == farmId)
                .Include(vs => vs.Stage)
                .ThenInclude(s => s.FarmingBatch)
                .ThenInclude(fb => fb.Cage)
                .AsQueryable();

            var vaccineScheduleStats = new VaccineScheduleStatisticModel
            {
                // Tổng số Vaccine Schedules
                TotalVaccineSchedules = await query.CountAsync(),

                // Vaccine Schedules đang "Upcoming"
                UpcomingVaccineSchedules = await query
                    .Where(vs => vs.Status == VaccineScheduleStatusEnum.Upcoming)
                    .CountAsync(),

                // Vaccine Schedules đã "Completed"
                CompletedVaccineSchedules = await query
                    .Where(vs => vs.Status == VaccineScheduleStatusEnum.Completed)
                    .CountAsync(),

                // Vaccine Schedules bị "Missed"
                MissedVaccineSchedules = await query
                    .Where(vs => vs.Status == VaccineScheduleStatusEnum.Missed)
                    .CountAsync(),
                CancelledVacineSchedules = await query
                    .Where(vs => vs.Status == VaccineScheduleStatusEnum.Cancelled)
                    .CountAsync(),
                RedoVaccineSchedules= await query
                    .Where(vs => vs.Status == VaccineScheduleStatusEnum.Redo)
                    .CountAsync()
            };

            return vaccineScheduleStats;
        }

    }
}
