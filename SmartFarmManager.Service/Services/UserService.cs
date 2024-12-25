using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Auth;
using SmartFarmManager.Service.BusinessModels.Farm;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        //public async Task<UserProfileModel> GetUserProfileAsync(Guid userId)
        //{
        //    var user = await _unitOfWork.Users.FindByCondition(x=>x.Id== userId,false,x=>x.Role).FirstOrDefaultAsync();

        //    if (user == null)
        //    {
        //        return null;
        //    }

        //    return new UserProfileModel
        //    {
        //        Username = user.Username,
        //        FullName = user.FullName,
        //        Email = user.Email,
        //        PhoneNumber = user.PhoneNumber,
        //        IsActive = user.IsActive ?? false,
        //        Role = user.Role.RoleName
        //    };
        //}
        public async Task<Guid> CreateUserAsync(CreateUserModel request)
        {
            var user = new User
            {
                Username = request.Username,
                PasswordHash = SecurityUtil.Hash(request.Password),
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                RoleId = request.RoleId,
                IsActive = true,
                CreatedAt = DateTimeUtils.VietnamNow()
            };

            var userId = await _unitOfWork.Users.CreateAsync(user);
            await _unitOfWork.CommitAsync();

            return userId;
        }

        public async Task<UserProfileModel> GetUserProfileAsync(Guid userId)
        {
            var user = await _unitOfWork.Users
                .FindByCondition(u => u.Id == userId, false, u => u.Role)
                .FirstOrDefaultAsync();

            if (user == null) return null;

            return new UserProfileModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = user.Role.RoleName,
                IsActive = user.IsActive ?? false,
                CreatedAt = user.CreatedAt ?? DateTime.MinValue
            };
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersAsync(string? role, bool? isActive, string? search)
        {
            var query = _unitOfWork.Users.FindAll(false, u => u.Role);

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Role.RoleName == role);
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
            }

            var users = await query.ToListAsync();

            return users.Select(u => new UserModel
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Address = u.Address,
                Role = u.Role.RoleName,
                IsActive = u.IsActive ?? false
            });
        }

        public async Task<UserModel> GetUserByIdAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.FindByCondition(u => u.Id == userId, false, u => u.Role).FirstOrDefaultAsync();
            if (user == null) return null;

            return new UserModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = user.Role.RoleName,
                IsActive = user.IsActive ?? false
            };
        }

        public async Task<UserDetailsModel> GetUserDetailsAsync(Guid userId)
        {
            var user = await _unitOfWork.Users
                .FindByCondition(u => u.Id == userId, false, u => u.Role, u => u.FarmAdmins, u => u.CageStaffs)
                .FirstOrDefaultAsync();

            if (user == null) return null;

            var userDetails = new UserDetailsModel
            {
                UserId = user.Id,
                Role = user.Role.RoleName
            };

            if (user.Role.RoleName == "Admin Farm")
            {
                userDetails.FarmIds = user.FarmAdmins.Select(fa => fa.FarmId).ToList();
            }
            else if (user.Role.RoleName == "Staff Farm")
            {
                userDetails.CageIds = user.CageStaffs.Select(cs => cs.CageId).ToList();
                userDetails.FarmBatchIds = user.CageStaffs
                    .SelectMany(cs => cs.Cage.FarmingBatches)
                    .Select(fb => fb.Id)
                    .ToList();
            }

            return userDetails;
        }

        public async Task<PagedResult<FarmModel>> GetFarmsByAdminStaffIdAsync(Guid userId, int pageIndex, int pageSize)
        {
            var farmIds = await _unitOfWork.FarmsAdmins
                                           .FindByCondition(fa => fa.AdminId == userId)
                                           .Select(fa => fa.FarmId)
                                           .ToListAsync();

            var farmsQuery = _unitOfWork.Farms.FindByCondition(f => farmIds.Contains(f.Id));

            var totalCount = await farmsQuery.CountAsync();
            var farms = await farmsQuery.Skip((pageIndex - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();
            // Ánh xạ Farm thành FarmModel
            var farmModels = farms.Select(f => new FarmModel
            {
                Id = f.Id,
                Name = f.Name,
                Address = f.Address,
                Area = f.Area,
                PhoneNumber = f.PhoneNumber,
                Email = f.Email
            }).ToList();
            var result= new PaginatedList<FarmModel>(farmModels, totalCount, pageIndex, pageSize);
            return new PagedResult<FarmModel>()
            {
                TotalItems = result.TotalCount,
                TotalPages = result.TotalPages,
                CurrentPage = result.CurrentPage,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage,
                Items = result.Items,
                PageSize = result.PageSize,
            };
        }

    }
}
