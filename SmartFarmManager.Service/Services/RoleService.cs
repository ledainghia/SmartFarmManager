using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Role;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<RoleModel>> GetRolesAsync()
        {
            var roles = await _unitOfWork.Roles.FindAll().ToListAsync();
            return roles.Select(role => new RoleModel
            {
                Id = role.Id,
                RoleName = role.RoleName
            });
        }
    }

}
