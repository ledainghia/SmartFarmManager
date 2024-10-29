using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserPermissionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Implement methods for UserPermission operations here
    }
}
