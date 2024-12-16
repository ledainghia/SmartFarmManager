using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.GrowthStageTemplate;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.TaskDailyTemplate;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFarmManager.Service.Helpers;

namespace SmartFarmManager.Service.Services
{
    public class TaskDailyTemplateService:ITaskDailyTemplateService
    {
        private IUnitOfWork _unitOfWork;

        public TaskDailyTemplateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateTaskDailyTemplateAsync(CreateTaskDailyTemplateModel model)
        {
            var growthStageTemplate = await _unitOfWork.GrowthStageTemplates
                .FindByCondition(g => g.Id == model.GrowthStageTemplateId)
                .FirstOrDefaultAsync();

            if (growthStageTemplate == null)
            {
                throw new ArgumentException($"Growth Stage Template with ID {model.GrowthStageTemplateId} does not exist.");
            }

            if (model.TaskTypeId.HasValue)
            {
                var taskType = await _unitOfWork.TaskTypes
                    .FindByCondition(t => t.Id == model.TaskTypeId)
                    .FirstOrDefaultAsync();

                if (taskType == null)
                {
                    throw new ArgumentException($"Task Type with ID {model.TaskTypeId} does not exist.");
                }
            }
            var duplicateTaskNameExists = await _unitOfWork.TaskDailyTemplates
                .FindByCondition(t => t.GrowthStageTemplateId == model.GrowthStageTemplateId && t.TaskName == model.TaskName)
                .AnyAsync();

            if (duplicateTaskNameExists)
            {
                throw new InvalidOperationException($"Task Name '{model.TaskName}' already exists in Growth Stage Template.");
            }

            if (model.TaskTypeId.HasValue)
            {
                var duplicateTaskTypeExists = await _unitOfWork.TaskDailyTemplates
                    .FindByCondition(t => t.GrowthStageTemplateId == model.GrowthStageTemplateId &&
                                          t.Session == model.Session &&
                                          t.TaskTypeId == model.TaskTypeId.Value)
                    .AnyAsync();

                if (duplicateTaskTypeExists)
                {
                    throw new InvalidOperationException($"Task Type '{model.TaskTypeId}' already exists in session {model.Session} of Growth Stage Template.");
                }
            }
            var taskDailyTemplate = new TaskDailyTemplate
            {
                GrowthStageTemplateId = model.GrowthStageTemplateId,
                TaskTypeId = model.TaskTypeId,
                TaskName = model.TaskName,
                Description = model.Description,
                Session = model.Session
            };

            await _unitOfWork.TaskDailyTemplates.CreateAsync(taskDailyTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> UpdateTaskDailyTemplateAsync(UpdateTaskDailyTemplateModel model)
        {
            var taskDailyTemplate = await _unitOfWork.TaskDailyTemplates
                .FindByCondition(t => t.Id == model.Id)
                .FirstOrDefaultAsync();

            if (taskDailyTemplate == null)
            {
                throw new ArgumentException($"Task Daily Template with ID {model.Id} does not exist.");
            }
            if (model.TaskTypeId.HasValue)
            {
                var taskType = await _unitOfWork.TaskTypes
                    .FindByCondition(t => t.Id == model.TaskTypeId)
                    .FirstOrDefaultAsync();

                if (taskType == null)
                {
                    throw new ArgumentException($"Task Type with ID {model.TaskTypeId} does not exist.");
                }
                if (model.Session.HasValue || taskDailyTemplate.Session != model.Session)
                {
                    var duplicateTaskTypeExists = await _unitOfWork.TaskDailyTemplates
                        .FindByCondition(t => t.GrowthStageTemplateId == taskDailyTemplate.GrowthStageTemplateId &&
                                              t.Session == (model.Session ?? taskDailyTemplate.Session) &&
                                              t.TaskTypeId == model.TaskTypeId &&
                                              t.Id != model.Id)
                        .AnyAsync();

                    if (duplicateTaskTypeExists)
                    {
                        throw new InvalidOperationException($"Task Type '{model.TaskTypeId}' already exists in the session.");
                    }
                }

                taskDailyTemplate.TaskTypeId = model.TaskTypeId;
            }
            if (!string.IsNullOrEmpty(model.TaskName) && taskDailyTemplate.TaskName != model.TaskName)
            {
                var duplicateTaskNameExists = await _unitOfWork.TaskDailyTemplates
                    .FindByCondition(t => t.GrowthStageTemplateId == taskDailyTemplate.GrowthStageTemplateId &&
                                          t.TaskName == model.TaskName &&
                                          t.Id != model.Id)
                    .AnyAsync();

                if (duplicateTaskNameExists)
                {
                    throw new InvalidOperationException($"Task Name '{model.TaskName}' already exists in Growth Stage Template.");
                }

                taskDailyTemplate.TaskName = model.TaskName;
            }
            if (!string.IsNullOrEmpty(model.Description))
            {
                taskDailyTemplate.Description = model.Description;
            }

            if (model.Session.HasValue)
            {
                taskDailyTemplate.Session = model.Session.Value;
            }
            await _unitOfWork.TaskDailyTemplates.UpdateAsync(taskDailyTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> DeleteTaskDailyTemplateAsync(Guid id)
        {
            var taskDailyTemplate = await _unitOfWork.TaskDailyTemplates
                .FindByCondition(t => t.Id == id)
                .FirstOrDefaultAsync();

            if (taskDailyTemplate == null)
            {
                return false; 
            }

            await _unitOfWork.TaskDailyTemplates.DeleteAsync(taskDailyTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<PagedResult<TaskDailyTemplateItemModel>> GetTaskDailyTemplatesAsync(TaskDailyTemplateFilterModel filter)
        {
            var query = _unitOfWork.TaskDailyTemplates
                .FindAll(false)
                .AsQueryable();

            if (filter.GrowthStageTemplateId.HasValue)
            {
                query = query.Where(t => t.GrowthStageTemplateId == filter.GrowthStageTemplateId.Value);
            }

            if (!string.IsNullOrEmpty(filter.TaskName))
            {
                query = query.Where(t => t.TaskName.Contains(filter.TaskName));
            }

            if (filter.Session.HasValue)
            {
                query = query.Where(t => t.Session == filter.Session.Value);
            }

            var totalItems = await query.CountAsync();
            var taskTypeIds = await query
                .Where(t => t.TaskTypeId.HasValue)
                .Select(t => t.TaskTypeId.Value)
                .Distinct()
                .ToListAsync();

            var taskTypes = await _unitOfWork.TaskTypes
                .FindByCondition(t => taskTypeIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => new TaskTypeResponse
                {
                    Id = t.Id,
                    TaskTypeName = t.TaskTypeName,
                    PriorityNum = t.PriorityNum
                });

            var items = await query
                .OrderBy(t => t.Session)
                .ThenBy(t => t.TaskName)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(t => new TaskDailyTemplateItemModel
                {
                    Id = t.Id,
                    GrowthStageTemplateId = t.GrowthStageTemplateId,
                    TaskTypeId = t.TaskTypeId,
                    TaskName = t.TaskName,
                    Description = t.Description,
                    Session = t.Session,
                    TaskType = t.TaskTypeId.HasValue && taskTypes.ContainsKey(t.TaskTypeId.Value)
                        ? taskTypes[t.TaskTypeId.Value]
                        : null
                })
                .ToListAsync();

            var result = new PaginatedList<TaskDailyTemplateItemModel>(items,totalItems,filter.PageNumber,filter.PageSize);

            return new PagedResult<TaskDailyTemplateItemModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage=result.HasNextPage,
                HasPreviousPage=result.HasPreviousPage,
            };
        }

        public async Task<TaskDailyTemplateDetailModel?> GetTaskDailyTemplateDetailAsync(Guid id)
        {
            var taskDailyTemplate = await _unitOfWork.TaskDailyTemplates
                .FindByCondition(t => t.Id == id)
                .FirstOrDefaultAsync();

            if (taskDailyTemplate == null)
            {
                return null; 
            }
            TaskTypeResponse? taskTypeResponse = null;
            if (taskDailyTemplate.TaskTypeId.HasValue)
            {
                var taskType = await _unitOfWork.TaskTypes
                    .FindByCondition(t => t.Id == taskDailyTemplate.TaskTypeId.Value)
                    .FirstOrDefaultAsync();

                if (taskType != null)
                {
                    taskTypeResponse = new TaskTypeResponse
                    {
                        Id = taskType.Id,
                        TaskTypeName = taskType.TaskTypeName,
                        PriorityNum = taskType.PriorityNum
                    };
                }
            }

            // 3. Map dữ liệu vào response
            return new TaskDailyTemplateDetailModel
            {
                Id = taskDailyTemplate.Id,
                GrowthStageTemplateId = taskDailyTemplate.GrowthStageTemplateId,
                TaskTypeId = taskDailyTemplate.TaskTypeId,
                TaskName = taskDailyTemplate.TaskName,
                Description = taskDailyTemplate.Description,
                Session = taskDailyTemplate.Session,
                TaskType = taskTypeResponse
            };
        }


    }
}
