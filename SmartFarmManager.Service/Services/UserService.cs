using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Auth;
using SmartFarmManager.Service.BusinessModels.Farm;
using SmartFarmManager.Service.BusinessModels.User;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime()
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

        public async Task<IEnumerable<BusinessModels.Auth.UserModel>> GetAllUsersAsync(string? role, bool? isActive, string? search)
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

            return users.Select(u => new BusinessModels.Auth.UserModel
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

        public async Task<BusinessModels.Auth.UserModel> GetUserByIdAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.FindByCondition(u => u.Id == userId, false, u => u.Role).FirstOrDefaultAsync();
            if (user == null) return null;

            return new BusinessModels.Auth.UserModel
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

        public async Task<Guid?> GetAssignedUserForCageAsync(Guid cageId, DateOnly date)
        {
            // Lấy ngày bắt đầu và ngày kết thúc của hôm nay
            var startDate = date.ToDateTime(new TimeOnly(0, 0));
            var endDate = date.ToDateTime(new TimeOnly(23, 59, 59));
            //get user by cageId
            var user = await _unitOfWork.CageStaffs.FindByCondition(c => c.CageId == cageId).FirstOrDefaultAsync();
            // Kiểm tra trong bảng TemporaryCageAssignment
            var temporaryAssignment = await _unitOfWork.LeaveRequests.FindByCondition(
                tca => tca.StaffFarmId == user.StaffFarmId &&
                       tca.StartDate <= endDate &&
                       tca.EndDate >= startDate
            ).FirstOrDefaultAsync();

            // Nếu tìm thấy nhân viên tạm thời, trả về TemporaryStaffId
            if (temporaryAssignment != null)
            {
                return temporaryAssignment.UserTempId;
            }

            // Nếu không tìm thấy trong TemporaryCageAssignment, lấy nhân viên chính từ CageStaff
            var cageStaff = await _unitOfWork.CageStaffs.FindByCondition(
                cs => cs.CageId == cageId
            ).FirstOrDefaultAsync();

            return cageStaff?.StaffFarmId;
        }

        public async Task<bool> UpdateUserDeviceIdAsync(Guid userId, string deviceId)
        {
            // Lấy thông tin người dùng từ database
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            // Kiểm tra người dùng có tồn tại không
            if (user == null)
            {
                throw new ArgumentException("Không tìm thấy người dùng");
            }

            // Cập nhật DeviceId mới
            user.DeviceId = deviceId;

            // Cập nhật thông tin người dùng
            await _unitOfWork.Users.UpdateAsync(user);

            // Lưu thay đổi vào database
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<BusinessModels.User.UserModel> CreateUserAsync(UserCreateModel request)
        {
            var existingUser = await _unitOfWork.Users
                .FindByCondition(u => u.Username == request.Username)
                .FirstOrDefaultAsync();

            if (existingUser != null)
                throw new ArgumentException("Username already exists.");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                RoleId = request.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = SecurityUtil.Hash("123@123Aa")
            };

            await _unitOfWork.Users.CreateAsync(newUser);
            await _unitOfWork.CommitAsync();

            return new BusinessModels.User.UserModel
            {
                Id = newUser.Id,
                Username = newUser.Username,
                FullName = newUser.FullName,
                Email = newUser.Email,
                PhoneNumber = newUser.PhoneNumber,
                Address = newUser.Address,
                IsActive = newUser.IsActive,
                CreatedAt = newUser.CreatedAt,
                RoleId = newUser.RoleId
            };
        }

        public async Task<bool> UpdateUserAsync(Guid userId, UserUpdateModel request)
        {
            var user = await _unitOfWork.Users.FindByCondition(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return false;

            if (!IsValidEmail(request.Email))
                throw new ArgumentException("Invalid email format.");

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.Address = request.Address;

            await _unitOfWork.Users.UpdateAsync(user);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> UpdatePasswordAsync(Guid userId, PasswordUpdateModel request)
        {
            var user = await _unitOfWork.Users.FindByCondition(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return false;

            if (user.PasswordHash != SecurityUtil.Hash(request.CurrentPassword))
                throw new ArgumentException("Current password is incorrect.");

            if (!IsValidPassword(request.NewPassword))
                throw new ArgumentException("Password must contain at least 8 characters, including uppercase, lowercase, number, and special character.");

            user.PasswordHash = SecurityUtil.Hash(request.NewPassword);
            await _unitOfWork.Users.UpdateAsync(user);

            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.FindByCondition(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return false;

            await _unitOfWork.Users.DeleteAsync(user);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<IEnumerable<BusinessModels.User.UserModel>> GetUsersAsync()
        {
            var users = await _unitOfWork.Users.FindAll().ToListAsync();
            return users.Select(user => new BusinessModels.User.UserModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                RoleId = user.RoleId
            });
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsValidPassword(string password)
        {
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(ch => !char.IsLetterOrDigit(ch));
        }

        public async Task<bool?> CheckUserByEmail(string email)
        {
            var checkUser = await _unitOfWork.Users.FindByCondition(u => u.Email == email).FirstOrDefaultAsync();
            if (checkUser == null)
            {
                return false;
            }
            return true;
        }
    }
}
