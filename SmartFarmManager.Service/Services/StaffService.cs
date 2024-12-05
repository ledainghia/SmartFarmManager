using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Staff;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class StaffService : IStaffService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StaffService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<StaffPendingTasksModel>> GetStaffSortedByPendingTasksAsync(Guid? cageId = null)
        {
            // Lấy danh sách tất cả các Staff và liên kết với CageStaffs và Cages
            var staffUsers = await _unitOfWork.Users.FindAll()
                .Where(u => u.Role.RoleName == "Staff Farm")
                .Include(s => s.CageStaffs)
                .ThenInclude(cs => cs.Cage)
                .ToListAsync();

            // Lấy danh sách công việc đang pending
            var pendingTasksQuery = _unitOfWork.Tasks.FindAll()
                .Where(t => t.Status == "Pending");

            // Nếu có CageId, lọc theo CageId
            if (cageId.HasValue)
            {
                pendingTasksQuery = pendingTasksQuery.Where(t => t.CageId == cageId.Value);
            }

            var pendingTasks = await pendingTasksQuery.ToListAsync();

            // Ghép danh sách nhân viên với Cage và số lượng task pending
            var result = staffUsers.SelectMany(staff => staff.CageStaffs.Select(cs => new StaffPendingTasksModel
            {
                StaffId = staff.Id,
                FullName = staff.FullName,
                Email = staff.Email,
                PhoneNumber = staff.PhoneNumber,
                CageId = cs.CageId,
                CageName = cs.Cage?.Name ?? "Unknown Cage",
                PendingTaskCount = pendingTasks.Count(t => t.AssignedToUserId == staff.Id && t.CageId == cs.CageId)
            }))
            .Where(r => !cageId.HasValue || r.CageId == cageId.Value) // Lọc theo CageId nếu có
            .OrderBy(r => r.PendingTaskCount) // Sắp xếp theo số lượng task pending
            .ToList();

            return result;
        }


    }
}
