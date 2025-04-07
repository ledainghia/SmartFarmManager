﻿using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Auth;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.BusinessModels.Staff;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using Sprache;
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
        public async Task<(bool Success, string Message)> AssignStaffToCageAsync(Guid userId, Guid cageId)
        {
            // Check if user exists and is Staff Farm
            var user = await _unitOfWork.Users.FindByCondition(u => u.Id == userId && u.Role.RoleName == "Staff Farm").FirstOrDefaultAsync();
            if (user == null)
            {
                return (false, "User is not a valid Staff Farm.");
            }

            // Check if cage exists
            var cage = await _unitOfWork.Cages.GetByIdAsync(cageId);
            if (cage == null)
            {
                return (false, "Cage not found.");
            }

            // Check if cage is already assigned
            var isAssigned = await _unitOfWork.CageStaffs.FindByCondition(cs => cs.CageId == cageId && cs.StaffFarmId==userId).AnyAsync();
            if (isAssigned)
            {
                return (false, "Cage is already assigned to a staff.");
            }
            var count = await _unitOfWork.CageStaffs.FindByCondition(cs => cs.StaffFarmId == userId).CountAsync();
            var farmConfig = await _unitOfWork.FarmConfigs.FindAll().FirstOrDefaultAsync();
            if(count >= farmConfig.MaxCagesPerStaff)
            {
                return (false, "Staff has reached the maximum number of cages.");
            }
            // Assign staff to the cage
            var cageStaff = new CageStaff
            {
                CageId = cageId,
                StaffFarmId = userId,
                AssignedDate = DateTimeUtils.GetServerTimeInVietnamTime()
            };

            await _unitOfWork.CageStaffs.CreateAsync(cageStaff);
            await _unitOfWork.CommitAsync();

            return (true, "Success");
        }
        public async Task<PagedResult<UserModel>> GetStaffFarmsByFarmIdAsync(Guid farmId, int pageIndex, int pageSize)
        {
            var cageIds = await _unitOfWork.Cages
                                           .FindByCondition(c => c.FarmId == farmId)
                                           .Select(c => c.Id)
                                           .ToListAsync();

            // Truy vấn CageStaffs, bao gồm StaffFarm và Cage
            var usersQuery = _unitOfWork.CageStaffs
                                        .FindByCondition(cs => cageIds.Contains(cs.CageId))                                       
                                        .Include(cs => cs.StaffFarm)
                                        .ThenInclude(s=>s.Role)                                      
                                        .Include(cs => cs.Cage)
                                        .Select(cs => new
                                        {
                                            StaffFarm = cs.StaffFarm,
                                            CageId = cs.CageId,
                                            CageName = cs.Cage.Name
                                        });

            var vetUsers = await _unitOfWork.Users.FindByCondition(u => u.Role.RoleName == "Vet")
                .Include(u => u.Role).FirstOrDefaultAsync();
            var adminFarmUsers = await _unitOfWork.FarmsAdmins.FindByCondition(fa => fa.FarmId == farmId)
                .Include(fa => fa.Admin)
                .ThenInclude(u => u.Role)
                .Select(fa => fa.Admin)
                .FirstOrDefaultAsync();
            var groupedUsers = await usersQuery
                .GroupBy(u => u.StaffFarm)
                .Select(group => new
                {
                    StaffFarm = group.Key,
                    Cages = group.Select(c => new
                    {
                        CageId = c.CageId,
                        CageName = c.CageName
                    }).ToList()
                })
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await usersQuery
                .GroupBy(u => u.StaffFarm)
                .CountAsync();

            // Ánh xạ sang UserModel
            var userModels = groupedUsers.Select(g => new UserModel
            {
                Id = g.StaffFarm.Id,
                Username = g.StaffFarm.Username,
                FullName = g.StaffFarm.FullName,
                Email = g.StaffFarm.Email,
                PhoneNumber = g.StaffFarm.PhoneNumber,
                Address = g.StaffFarm.Address,
                Role = g.StaffFarm.Role != null ? g.StaffFarm.Role.RoleName : "Staff Farm",
                IsActive = g.StaffFarm.IsActive ?? false,
                Cages = g.Cages.Select(c => new CageModel
                {
                    Id = c.CageId,
                    Name = c.CageName
                }).ToList(),
                TasksCountByStatus = new TaskStatusCountModel
                {
                    Pending = _unitOfWork.Tasks.FindByCondition(x=>x.AssignedToUserId==g.StaffFarm.Id && x.Status == "Pending").Count(),
                    InProgress = _unitOfWork.Tasks.FindByCondition(x => x.AssignedToUserId == g.StaffFarm.Id && x.Status == "InProgress").Count(),
                    Done = _unitOfWork.Tasks.FindByCondition(x => x.AssignedToUserId == g.StaffFarm.Id && x.Status == "Done").Count(),
                    Overdue = _unitOfWork.Tasks.FindByCondition(x => x.AssignedToUserId == g.StaffFarm.Id && x.Status == "Overdue").Count(),
                }
            }).ToList();

            if (vetUsers != null)
            {
                userModels.Add(new UserModel
                {
                    Id = vetUsers.Id,
                    Username = vetUsers.Username,
                    FullName = vetUsers.FullName,
                    Email = vetUsers.Email,
                    PhoneNumber = vetUsers.PhoneNumber,
                    Address = vetUsers.Address,
                    Role = vetUsers.Role != null ? vetUsers.Role.RoleName : "Vet",
                    IsActive = vetUsers.IsActive ?? false,
                    Cages = null,
                    TasksCountByStatus = null 
                });
            }

            if (adminFarmUsers != null )
            {
                userModels.Add(new UserModel
                {
                    Id = adminFarmUsers.Id,
                    Username = adminFarmUsers.Username,
                    FullName = adminFarmUsers.FullName,
                    Email = adminFarmUsers.Email,
                    PhoneNumber = adminFarmUsers.PhoneNumber,
                    Address = adminFarmUsers.Address,
                    Role = adminFarmUsers.Role != null ? adminFarmUsers.Role.RoleName : "Admin Farm",
                    IsActive = adminFarmUsers.IsActive ?? false,
                    Cages = null,
                    TasksCountByStatus = null
                });
            }
            var result= new PaginatedList<UserModel>(userModels, totalCount, pageIndex, pageSize);
            return new PagedResult<UserModel>()
            {
                Items = result.Items,
                CurrentPage = result.CurrentPage,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage,
                PageSize = result.PageSize,
                TotalItems = result.TotalCount,
                TotalPages = result.TotalPages,
            };
        }


    }
}
