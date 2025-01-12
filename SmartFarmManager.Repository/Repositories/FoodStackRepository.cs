using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Repository.Repositories
{
    public class FoodStackRepository : RepositoryBaseAsync<FoodStack>, IFoodStackRepository
    {
        public FoodStackRepository(SmartFarmContext dbContext) : base(dbContext)
        {
        }
    }
}
