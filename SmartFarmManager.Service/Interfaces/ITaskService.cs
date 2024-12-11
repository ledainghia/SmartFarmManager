using SmartFarmManager.Service.BusinessModels;
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
        Task<bool> ChangeTaskStatusAsync(Guid taskId, Guid statusId);
        Task<TaskDetailModel> GetTaskDetailAsync(Guid taskId);
        Task<List<TaskModel>> GetTasksAsync(TaskModel taskModel);
        Task<PagedResult<TaskDetailModel>> GetFilteredTasksAsync(TaskFilterModel filter);

        Task<List<TaskResponse>> GetTasksForUserWithStateAsync(Guid userId, Guid cageId, DateTime? dateTime = null);

        Task<List<NextTaskModel>> GetNextTasksForCagesWithStatsAsync(Guid userId);

        Task<bool> UpdateTaskPrioritiesAsync(List<TaskPriorityUpdateModel> taskPriorityUpdateModels);
        Task<List<SessionTaskGroupModel>> GetUserTasksAsync(Guid userId, DateTime? filterDate = null);
    }
}
