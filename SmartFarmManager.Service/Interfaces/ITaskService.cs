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
        Task<bool> CreateTaskAsync(CreateTaskModel model);
        Task<bool> UpdateTaskPriorityAsync(Guid taskId, UpdateTaskPriorityModel model);
    }
}
