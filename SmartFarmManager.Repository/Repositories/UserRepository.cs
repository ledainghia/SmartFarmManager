using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Repository.Repositories
{
    public class UserRepository : RepositoryBaseAsync<User>, IUserRepository
    {
        public UserRepository(FarmsContext dbContext) : base(dbContext)
        {
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _dbContext.Users.Include(u=>u.UserPermissions).ThenInclude(x=>x.Permission)
                .Include(x=>x.Roles).FirstOrDefaultAsync(x=>x.Id== id); 
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _dbContext.Users.Include(u=>u.Roles).FirstOrDefaultAsync(u=>u.Username == username);
        }
    }
}
