using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Repository.Repositories
{
    public class StatusLogRepository : RepositoryBaseAsync<DataAccessObject.Models.StatusLog>, IStatusLogRepository
    {
        public StatusLogRepository(SmartFarmContext dbContext) : base(dbContext)
        {
        }
    }
}
