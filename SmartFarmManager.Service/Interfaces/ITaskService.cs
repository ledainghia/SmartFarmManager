﻿using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.BusinessModels.VaccineSchedule;
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
        Task<bool> ChangeTaskStatusAsync(Guid taskId, string? status);
        Task<TaskDetailModel> GetTaskDetailAsync(Guid taskId);
        Task<List<TaskModel>> GetTasksAsync(TaskModel taskModel);
        Task<PagedResult<TaskDetailModel>> GetFilteredTasksAsync(TaskFilterModel filter);

        Task<List<TaskResponse>> GetTasksForUserWithStateAsync(Guid userId, Guid cageId, DateTime? dateTime = null);

        Task<List<NextTaskModel>> GetNextTasksForCagesWithStatsAsync(Guid userId);

        Task<bool> UpdateTaskPrioritiesAsync(List<TaskPriorityUpdateModel> taskPriorityUpdateModels);
        Task<List<SessionTaskGroupModel>> GetUserTasksAsync(Guid userId, DateTime? filterDate = null, Guid? cageId = null);
        Task<bool> UpdateTaskAsync(TaskDetailUpdateModel model);
        Task<bool> CreateTaskRecurringAsync(CreateTaskRecurringModel model);
        Task<bool> GenerateTasksForTodayAsync();
        Task<bool> UpdateAllTaskStatusesAsync();
        Task<bool> GenerateTasksForFarmingBatchAsync(Guid farmingBatchId);
        Task<bool> GenerateTasksForTomorrowAsync();
        Task<bool> UpdateEveningTaskStatusesAsync();
        Task<bool> GenerateTreatmentTasksAsync();
        Task<bool> GenerateTreatmentTasksAsyncV2();
        Task<Dictionary<string, int>> GetTaskCountByStatusAsync(DateTime startDate, DateTime endDate, Guid? assignedToUserId = null, Guid? farmId = null);
        Task<bool> RedoVaccineScheduleAsync(RedoVaccineScheduleRequest request);
        Task<bool> SetIsTreatmentTaskTrueAsync(Guid taskId,Guid MedicalSymptomId);
        Task<TaskLogResponse> GetLogsByTaskIdAsync(Guid taskId);


    }
}
