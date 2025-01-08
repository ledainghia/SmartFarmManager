using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Repository.Repositories
{
    public class DiseaseRepository : RepositoryBaseAsync<Disease>, IDiseaseRepositoy
    {
        public DiseaseRepository(SmartFarmContext dbContext) : base(dbContext)
        {
        }
    }
}
