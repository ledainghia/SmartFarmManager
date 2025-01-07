using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Repository.Repositories
{
    public class StandardPrescriptionRepository : RepositoryBaseAsync<StandardPrescription>, IStandardPrescriptionRepository
    {
        public StandardPrescriptionRepository(SmartFarmContext dbContext) : base(dbContext)
        {
        }
    }
}
