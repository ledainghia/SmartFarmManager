using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Auth;
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

        // Implement methods for User operations here
        public async Task<UserProfileModel> GetUserProfileAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            var roles = user.Roles.Select(r => r.RoleName).ToList();
            var permissions = user.UserPermissions.Select(up => up.Permission.PermissionName).ToList();

            return new UserProfileModel
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive ?? false,
                Roles = roles,
                Permissions = permissions
            };
        }
    }
}
