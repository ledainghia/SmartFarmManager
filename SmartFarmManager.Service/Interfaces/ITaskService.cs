using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface ITaskService
    {
        Task<DataAccessObject.Models.Task> CreateTaskAsync(CreateTaskModel model);
    }
}
