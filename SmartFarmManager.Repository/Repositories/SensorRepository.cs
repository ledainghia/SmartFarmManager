using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Repository.Repositories
{
    public class SensorRepository : RepositoryBaseAsync<Sensor>, ISensorRepository
    {
        public SensorRepository(SmartFarmContext dbContext) : base(dbContext)
        {
        }
    }
}
