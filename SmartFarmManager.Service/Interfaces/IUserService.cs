﻿using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Auth;
using SmartFarmManager.Service.BusinessModels.Farm;
using SmartFarmManager.Service.BusinessModels.Users;
using SmartFarmManager.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileModel> GetUserProfileAsync(Guid userId);
        Task<IEnumerable<BusinessModels.Auth.UserModel>> GetAllUsersAsync(string? role, bool? isActive, string? search);
        Task<BusinessModels.Auth.UserModel> GetUserByIdAsync(Guid userId);
        Task<Guid> CreateUserAsync(CreateUserModel userModel);
        Task<UserDetailsModel> GetUserDetailsAsync(Guid userId);
        Task<PagedResult<FarmModel>> GetFarmsByAdminStaffIdAsync(Guid userId, int pageIndex, int pageSize);
        Task<Guid?> GetAssignedUserForCageAsync(Guid cageId, DateOnly date);
        Task<bool> UpdateUserDeviceIdAsync(Guid userId, string deviceId);

        Task<BusinessModels.Users.UserModel> CreateUserAsync(UserCreateModel request);
        Task<bool> UpdateUserAsync(Guid userId, UserUpdateModel request);
        Task<bool> UpdatePasswordAsync(Guid userId, PasswordUpdateModel request);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<IEnumerable<BusinessModels.Users.UserModel>> GetUsersAsync();

        Task<bool?> CheckUserByEmail(string email, string username);
        Task<bool?> CheckUserByPhone(string phone, string username);
        Task<PagedResult<BusinessModels.Users.UserModel>> GetUsersAsync(UserFilterModel filter);
        Task<IEnumerable<BusinessModels.Users.UserModel>> GetUsersAsync(string? roleName, bool? isActive, string? search);
        Task<bool> VerifyPasswordAsync(UserPasswordRequest request);
        Task<bool> ResetPasswordAsync(UserPasswordRequest request);
        Task<bool> AssignCagesToStaffAsync(Guid staffId, List<Guid> newCageIds);
        Task<bool> ToggleUserStatusAsync(Guid userId);
    }
}
