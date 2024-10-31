using SmartFarmManager.DataAccessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Repository.Interfaces
{
    public interface IFarmStaffAssignmentRepository : IRepositoryBaseAsync<FarmStaffAssignment>
    {
        Task<List<FarmStaffAssignment>> GetByFarmIdAsync(int farmId);
    }
}
