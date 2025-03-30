using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Auth;
using SmartFarmManager.Service.BusinessModels.Farm;
using SmartFarmManager.Service.BusinessModels.Users;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using Sprache;
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
                CreatedAt = user.CreatedAt ?? DateTime.MinValue,
                ImageUrl = user.ImageURL
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
        public async Task<BusinessModels.Users.UserModel> CreateUserAsync(UserCreateModel request)
        {

            // Kiểm tra Role
            var role = await _unitOfWork.Roles.FindByCondition(r => r.Id == request.RoleId).FirstOrDefaultAsync();
            if (role == null)
                throw new ArgumentException("Invalid Role ID.");

            // Nếu Role là Vet hoặc AdminFarm, chỉ có 1 tài khoản active
            if (role.RoleName == "Vet" || role.RoleName == "AdminFarm")
            {
                var activeUserCount = await _unitOfWork.Users
                    .FindByCondition(u => u.RoleId == request.RoleId && u.IsActive == true)
                    .CountAsync();

                if (activeUserCount > 0)
                    throw new ArgumentException($"{role.RoleName} can only have one active account.");
            }
            var exitingEmail = await _unitOfWork.Users.FindByCondition(u => u.Email == request.Email).FirstOrDefaultAsync();
            if(exitingEmail != null)
            {
                throw new ArgumentException($"{request.Email} already exit.");
            }
            // Tạo Username tự động
            var username = await GenerateUniqueUsernameAsync(request.FullName);

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = username, // Tạo username mới
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                RoleId = request.RoleId,
                IsActive = true,
                CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                PasswordHash = SecurityUtil.Hash("123@123Aa")
            };

            await _unitOfWork.Users.CreateAsync(newUser);
            await _unitOfWork.CommitAsync();

            return new BusinessModels.Users.UserModel
            {
                Id = newUser.Id,
                Username = username,
                FullName = newUser.FullName,
                Email = newUser.Email,
                PhoneNumber = newUser.PhoneNumber,
                Address = newUser.Address,
                IsActive = newUser.IsActive,
                CreatedAt = newUser.CreatedAt,
                RoleId = newUser.RoleId
            };
        }

        private async Task<string> GenerateUniqueUsernameAsync(string fullName)
        {
            // 1️⃣ Tách tên thành các phần
            var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length < 2)
                throw new ArgumentException("Full name must have at least two words.");

            // 2️⃣ Lấy họ cuối cùng
            string lastName = nameParts[^1];

            // 3️⃣ Lấy chữ cái đầu tiên của tất cả các phần còn lại
            string initials = string.Join("", nameParts.Take(nameParts.Length - 1).Select(n => n[0]));

            // 4️⃣ Kết hợp lại thành username
            string usernameBase = RemoveDiacritics(lastName.ToLower() + initials.ToLower());

            // 5️⃣ Kiểm tra số lượng username có cùng tiền tố trong database
            var existingUsersCount = await _unitOfWork.Users
                .FindByCondition(u => u.Username.StartsWith(usernameBase))
                .CountAsync();

            return $"{usernameBase}{existingUsersCount + 1}";
        }

        private string RemoveDiacritics(string text)
        {
            // Thay thế đặc biệt cho chữ Đ/đ trước khi loại dấu
            text = text.Replace("Đ", "D").Replace("đ", "d");
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
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
            user.ImageURL = request.ImageUrl;

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

        public async Task<IEnumerable<BusinessModels.Users.UserModel>> GetUsersAsync()
        {
            var users = await _unitOfWork.Users.FindAll().ToListAsync();
            return users.Select(user => new BusinessModels.Users.UserModel
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

        public async Task<bool?> CheckUserByEmail(string email, string username)
        {
            var checkUser = await _unitOfWork.Users.FindByCondition(u => u.Email == email && u.Username == username).FirstOrDefaultAsync();
            if (checkUser == null)
            {
                return false;
            }
            return true;
        }

        public async Task<bool?> CheckUserByPhone(string phone, string username)
        {
            var checkUser = await _unitOfWork.Users.FindByCondition(u => u.PhoneNumber == phone && u.Username == username).FirstOrDefaultAsync();
            if (checkUser == null)
            {
                return false;
            }
            return true;
        }
        public async Task<PagedResult<BusinessModels.Users.UserModel>> GetUsersAsync(UserFilterModel filter)
        {
            var query = _unitOfWork.Users.FindAll()
                .Include(u => u.Role)
                .Include(u => u.CageStaffs)
                    .ThenInclude(cs => cs.Cage)
                        .ThenInclude(c => c.Farm)
                .Include(u => u.FarmAdmins)
                    .ThenInclude(fa => fa.Farm).AsQueryable(); ;

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Username))
                query = query.Where(u => u.Username.Contains(filter.Username));

            if (!string.IsNullOrWhiteSpace(filter.Email))
                query = query.Where(u => u.Email.Contains(filter.Email));

            if (!string.IsNullOrWhiteSpace(filter.PhoneNumber))
                query = query.Where(u => u.PhoneNumber.Contains(filter.PhoneNumber));

            if (filter.RoleId.HasValue)
                query = query.Where(u => u.RoleId == filter.RoleId.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.FullName))
                query = query.Where(u => u.FullName.Contains(filter.FullName));

            if (!string.IsNullOrWhiteSpace(filter.Address))
                query = query.Where(u => u.Address.Contains(filter.Address));

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string keyword = filter.Search.Trim().ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(keyword) ||
                    u.FullName.ToLower().Contains(keyword) ||
                    u.Email.ToLower().Contains(keyword) ||
                    u.PhoneNumber.ToLower().Contains(keyword) ||
                    u.Address.ToLower().Contains(keyword)
                );
            }

            // Pagination
            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(u => new BusinessModels.Users.UserModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    RoleId = u.RoleId,
                    RoleName = u.Role.RoleName,
                    FarmName = u.FarmAdmins.Select(fa => fa.Farm.Name)
                                 .Union(u.CageStaffs.Select(cs => cs.Cage.Farm.Name))
                                 .Distinct()
                                 .First(),
                    CageNames = u.CageStaffs.Select(cs => cs.Cage.Name).Distinct().ToList()
                })
                .ToListAsync();

            return new PagedResult<BusinessModels.Users.UserModel>
            {
                Items = items,
                TotalItems = totalItems,
                PageSize = filter.PageSize,
                CurrentPage = filter.PageNumber,
                TotalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize),
                HasNextPage = filter.PageNumber * filter.PageSize < totalItems,
                HasPreviousPage = filter.PageNumber > 1
            };
        }



        public async Task<IEnumerable<BusinessModels.Users.UserModel>> GetUsersAsync(string? roleName, bool? isActive, string? search)
        {
            var query = _unitOfWork.Users.FindAll();

            // Lọc theo RoleName
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                query = query.Where(u => u.Role.RoleName == roleName);
            }

            // Lọc theo trạng thái hoạt động
            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            // Tìm kiếm chung trên nhiều cột
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Username.Contains(search) ||
                    u.FullName.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.PhoneNumber.Contains(search) ||
                    u.Address.Contains(search) ||
                    u.Role.RoleName.Contains(search)
                );
            }

            var users = await query.ToListAsync();

            return users.Select(user => new BusinessModels.Users.UserModel
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

        /// <summary>
        /// Kiểm tra mật khẩu có đúng không
        /// </summary>
        public async Task<bool> VerifyPasswordAsync(UserPasswordRequest request)
        {
            var user = await _unitOfWork.Users.FindByCondition(u => u.Id == request.UserId).FirstOrDefaultAsync();

            if (user == null)
                throw new ArgumentException("User not found.");

            string hashedPassword = SecurityUtil.Hash(request.Password);
            return user.PasswordHash == hashedPassword;
        }

        /// <summary>
        /// Đặt lại mật khẩu mới
        /// </summary>
        public async Task<bool> ResetPasswordAsync(UserPasswordRequest request)
        {
            var user = await _unitOfWork.Users.FindByCondition(u => u.Id == request.UserId).FirstOrDefaultAsync();

            if (user == null)
                throw new ArgumentException("User not found.");

            user.PasswordHash = SecurityUtil.Hash(request.Password);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> AssignCagesToStaffAsync(Guid staffId, List<Guid> newCageIds)
        {
            // 1. Kiểm tra User
            var user = await _unitOfWork.Users
                .FindByCondition(u => u.Id == staffId)
                .Include(u => u.Role)
                .Include(u => u.CageStaffs)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new ArgumentException("Người dùng không tồn tại.");

            if (user.Role.RoleName != "Staff Farm")
                throw new InvalidOperationException("Chỉ nhân viên trang trại mới được gán chuồng.");

            // 2. Lấy FarmConfig
            var farmConfig = await _unitOfWork.FarmConfigs.FindAll().FirstOrDefaultAsync();
            if (farmConfig == null)
                throw new InvalidOperationException("Không tìm thấy cấu hình trang trại.");

            int maxCages = farmConfig.MaxCagesPerStaff;
            var distinctNewCageIds = newCageIds.Distinct().ToList();

            if (distinctNewCageIds.Count > maxCages)
                throw new InvalidOperationException($"Vượt quá số chuồng tối đa cho phép ({maxCages}). Đã chọn {distinctNewCageIds.Count} chuồng.");

            // 3. Xóa các chuồng cũ đã gán
            var oldAssignments = await _unitOfWork.CageStaffs
                .FindByCondition(cs => cs.StaffFarmId == staffId)
                .ToListAsync();

            if (oldAssignments.Any())
                await _unitOfWork.CageStaffs.DeleteListAsync(oldAssignments);

            // 4. Gán các chuồng mới
            var newAssignments = distinctNewCageIds.Select(cageId => new CageStaff
            {
                Id = Guid.NewGuid(),
                StaffFarmId = staffId,
                CageId = cageId,
                AssignedDate = DateTimeUtils.GetServerTimeInVietnamTime()
            }).ToList();

            await _unitOfWork.CageStaffs.CreateListAsync(newAssignments);
            await _unitOfWork.CommitAsync();

            return true;
        }


        public async Task<bool> ToggleUserStatusAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.FindByCondition(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new KeyNotFoundException($"Người dùng với Id là {userId} không tìm thấy !.");
            }

            // Toggle the IsActive status
            user.IsActive = !user.IsActive;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            return true;
        }

    }
}
