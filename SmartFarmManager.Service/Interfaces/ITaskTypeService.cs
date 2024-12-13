using SmartFarmManager.Service.BusinessModels.TaskType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface ITaskTypeService
    {
        Task<Guid> CreateTaskTypeAsync(TaskTypeModel taskTypeModel);
        Task<IEnumerable<TaskTypeModel>> GetTaskTypesAsync();
        Task<TaskTypeModel> GetTaskTypeByIdAsync(Guid id);
        Task<bool> UpdateTaskTypeAsync(TaskTypeModel taskTypeModel);
        Task<bool> DeleteTaskTypeAsync(Guid id);
    }

}
