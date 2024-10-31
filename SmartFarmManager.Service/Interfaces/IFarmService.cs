using SmartFarmManager.Service.BusinessModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IFarmService
    {
        Task<List<UserResponseModel>> GetUsersByFarmIdAsync(int farmId);
    }
}
