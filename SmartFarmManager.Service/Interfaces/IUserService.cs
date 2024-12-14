﻿using SmartFarmManager.Service.BusinessModels.Auth;
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
        Task<IEnumerable<UserModel>> GetAllUsersAsync(string? role, bool? isActive, string? search);
        Task<UserModel> GetUserByIdAsync(Guid userId);
        Task<Guid> CreateUserAsync(CreateUserModel userModel);
        Task<UserDetailsModel> GetUserDetailsAsync(Guid userId);

    }
}
