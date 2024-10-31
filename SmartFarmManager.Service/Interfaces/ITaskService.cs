using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.QueryParameters;
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
        System.Threading.Tasks.Task UpdateTaskAsync(int taskId, UpdateTaskModel model);
        System.Threading.Tasks.Task UpdateTaskStatusAsync(int taskId, string newStatus, int modifiedById);
        Task<TaskDetailModel?> GetTaskDetailAsync(int taskId);
        Task<PagedResult<TaskDetailModel>> GetAllTasksAsync(TasksQuery query);

    }
}
