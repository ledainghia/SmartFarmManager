using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.User;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class FarmService : IFarmService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FarmService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        // Implement methods for Farm operations here

        public async Task<List<UserResponseModel>> GetUsersByFarmIdAsync(int farmId)
        {
            var farm = await _unitOfWork.Farms.GetByIdAsync(farmId);
            if (farm == null)
            {
                throw new ArgumentException("Farm not found.");
            }

            // Lấy danh sách người dùng đã được gán cho nông trại
            var farmStaffAssignmentsExist = await _unitOfWork.FarmStaffAssignments.GetByFarmIdAsync(farmId);
            var users = farmStaffAssignmentsExist.Select(fsa => fsa.FarmStaff);
            


            // Chuyển đổi sang UserResponse
            var userResponses = users.Select(user => new UserResponseModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive
            }).ToList();

            return userResponses;
        }

        


    }
}
