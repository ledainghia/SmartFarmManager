using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Repository.Repositories
{
    public class DeviceReadingRepository : RepositoryBaseAsync<DeviceReading>, IDeviceReadingRepository
    {
        public DeviceReadingRepository(FarmsContext dbContext) : base(dbContext)
        {
        }
    }
}
