using Microsoft.EntityFrameworkCore;
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
        public async Task<UserProfileModel> GetUserProfileAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.FindByCondition(x=>x.Id== userId,false,x=>x.Role).FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            return new UserProfileModel
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive ?? false,
                Role = user.Role.RoleName
            };
        }
    }
}
