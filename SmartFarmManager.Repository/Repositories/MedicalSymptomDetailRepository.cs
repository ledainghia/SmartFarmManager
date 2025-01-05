using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Repository.Repositories
{
    public class MedicalSymptomDetailRepository : RepositoryBaseAsync<MedicalSymtomDetail>, IMedicalSymptomDetailRepository
    {
        public MedicalSymptomDetailRepository(SmartFarmContext dbContext) : base(dbContext)
        {
        }
    }
}
