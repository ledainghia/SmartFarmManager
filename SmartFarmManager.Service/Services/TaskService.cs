using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.QueryParameters;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Shared.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{

    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaskService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Implement methods for Task operations here

        public async Task<DataAccessObject.Models.Task> CreateTaskAsync(CreateTaskModel model)
        {
            if (model.TaskType == TaskTypeEnum.FARM && !model.FarmId.HasValue)
            {
                throw new ArgumentException("FarmId is required when TaskType is 'Farm'.");
            }

            if (model.FarmId.HasValue)
            {
                var farmExists = await _unitOfWork.Farms.FindAsync(x=>x.Id==model.FarmId);
                if (farmExists==null)
                {
                    throw new ArgumentException("The specified FarmId does not exist.");
                }
            }

         
            if (model.AssignedToUserId.HasValue)
            {
                var userExists = await _unitOfWork.Users.FindAsync(x=>x.Id==model.AssignedToUserId);
                if (userExists==null)
                {
                    throw new ArgumentException("The specified AssignedToUserId does not exist.");
                }
            }

            var newTask = new DataAccessObject.Models.Task
            {
                TaskName = model.TaskName,
                DueDate = model.DueDate,
                TaskType = model.TaskType,
                Description = model.Description,
                Status = TaskStatusEnum.TO_DO,
                FarmId = model.FarmId,
                AssignedToUserId = model.AssignedToUserId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = model.CreatedBy,
                ModifiedAt = null,
                ModifiedBy = null
            };

            await _unitOfWork.Tasks.CreateAsync(newTask); 
            await _unitOfWork.CommitAsync(); 

            return newTask;
        }

        public async Task<PagedResult<TaskDetailModel>> GetAllTasksAsync(TasksQuery query)
        {
            var tasksQuery = _unitOfWork.Tasks.FindAll(); // Giả sử có phương thức này trong repository

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(query.Status))
            {
                tasksQuery = tasksQuery.Where(t => t.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
            }

            // Lọc theo loại nhiệm vụ
            if (!string.IsNullOrEmpty(query.TaskType))
            {
                tasksQuery = tasksQuery.Where(t => t.TaskType.Equals(query.TaskType, StringComparison.OrdinalIgnoreCase));
            }

            // Sắp xếp
            tasksQuery = query.SortDescending
                ? tasksQuery.OrderByDescending(t => EF.Property<object>(t, query.SortBy))
                : tasksQuery.OrderBy(t => EF.Property<object>(t, query.SortBy));

            // Phân trang
            var totalCount = await tasksQuery.CountAsync();
            var tasks = await tasksQuery.Skip((query.PageIndex - 1) * query.PageSize)
                                         .Take(query.PageSize)
                                         .ToListAsync();

            // Chuyển đổi sang TaskDetailResponse
            var taskDetailResponses = tasks.Select(task => new TaskDetailModel
            {
                Id = task.Id,
                TaskName = task.TaskName,
                Description = task.Description,
                DueDate = (DateTime)task.DueDate,
                TaskType = task.TaskType,
                FarmId = task.FarmId,
                AssignedToUserId = task.AssignedToUserId,
                Status = task.Status,
                CompleteAt = task.CompletedAt,
                CreatedAt = (DateTime) task.CreatedAt,
                ModifiedAt = task.ModifiedAt
            }).ToList();

            // Trả về kết quả phân trang
            return new PagedResult<TaskDetailModel>
            {
                Items = taskDetailResponses,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
                CurrentPage = query.PageIndex,
                PageSize = query.PageSize
            };
        }

        public async Task<TaskDetailModel?> GetTaskDetailAsync(int taskId)
        {
            var task =  await _unitOfWork.Tasks.FindAsync(x=>x.Id==taskId);
            if (task == null)
            {
                return null;
            }

            var taskHistories = await _unitOfWork.TaskHistories.FindAllAsync(x=>x.TaskId==taskId);

            // Chuyển đổi sang TaskDetailModel
            var taskDetailModel = new TaskDetailModel
            {
                Id = task.Id,
                TaskName = task.TaskName,
                Description = task.Description,
                DueDate = (DateTime)task.DueDate,
                TaskType = task.TaskType,
                FarmId = task.FarmId,
                AssignedToUserId = task.AssignedToUserId,
                Status = task.Status,
                CompleteAt = task.CompletedAt,
                CreatedAt = (DateTime)task.CreatedAt,
                ModifiedAt = task.ModifiedAt,
                TaskHistories = taskHistories.Select(th => new TaskHistoryModel
                {
                    Id=th.Id,
                    StatusBefore = th.StatusBefore,
                    StatusAfter = th.StatusAfter,
                    ChangedAt =  (DateTime)th.ChangedAt
                }).ToList()
            };
            return taskDetailModel;

        }

        public async System.Threading.Tasks.Task UpdateTaskAsync(int taskId, UpdateTaskModel model)
        {
            var taskToUpdate = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (taskToUpdate == null)
            {
                throw new ArgumentException("Task not found.");
            }

            // Cập nhật các trường nếu chúng không null
            if (model.TaskName != null)
                taskToUpdate.TaskName = model.TaskName;

            if (model.Description != null)
                taskToUpdate.Description = model.Description;

            if (model.DueDate.HasValue)
                taskToUpdate.DueDate = model.DueDate.Value;

            if (model.TaskType != null)
                taskToUpdate.TaskType = model.TaskType;

            if (model.FarmId.HasValue)
            {
                var farmExists = await _unitOfWork.Farms.FindAsync(x=>x.Id==model.FarmId);
                if (farmExists==null)
                {
                    throw new ArgumentException("The specified FarmId does not exist.");
                }
                taskToUpdate.FarmId = model.FarmId.Value;
            }

            if (model.AssignedToUserId.HasValue)
            {
                var userExists = await _unitOfWork.Users.FindAsync(x=>x.Id==model.AssignedToUserId);
                if (userExists == null)
                {
                    throw new ArgumentException("The specified AssignedToUserId does not exist.");
                }
                taskToUpdate.AssignedToUserId = model.AssignedToUserId;
            }

            if (model.Status != null &&model.Status!=taskToUpdate.Status)
            {
                

                // Lưu lịch sử thay đổi trạng thái
                var taskHistory = new TaskHistory
                {
                    TaskId = taskToUpdate.Id,
                    StatusBefore = taskToUpdate.Status,
                    StatusAfter = model.Status,
                    ChangedAt = DateTime.UtcNow
                };
                taskToUpdate.Status = model.Status;
                await _unitOfWork.TaskHistories.CreateAsync(taskHistory);

                // Nếu status là "Done", cập nhật CompleteAt
                if (model.Status == TaskStatusEnum.DONE)
                {
                    taskToUpdate.CompletedAt = DateTime.UtcNow;
                }
            }


            taskToUpdate.ModifiedAt = DateTime.UtcNow;
            taskToUpdate.ModifiedBy = model.ModifiedBy;

            await _unitOfWork.Tasks.UpdateAsync(taskToUpdate); // Cập nhật nhiệm vụ vào DbSet
            await _unitOfWork.CommitAsync(); // Lưu thay đổi vào cơ sở dữ liệu

        
        }

        public async System.Threading.Tasks.Task UpdateTaskStatusAsync(int taskId, string newStatus, int modifiedById)
        {
            var taskToUpdate = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (taskToUpdate == null)
            {
                throw new ArgumentException("Task not found.");
            }

            // Lưu trạng thái cũ để so sánh
            var statusBefore = taskToUpdate.Status;

            // Cập nhật trạng thái
            taskToUpdate.Status = newStatus;

            // Lưu lịch sử thay đổi trạng thái
            var taskHistory = new TaskHistory
            {
                TaskId = taskToUpdate.Id,
                StatusBefore = statusBefore,
                StatusAfter = newStatus,
                ChangedAt = DateTime.UtcNow
            };
            await _unitOfWork.TaskHistories.CreateAsync(taskHistory);

            // Nếu trạng thái là "Done", cập nhật CompleteAt
            if (newStatus == TaskStatusEnum.DONE)
            {
                taskToUpdate.CompletedAt = DateTime.UtcNow;
            }

            taskToUpdate.ModifiedAt = DateTime.UtcNow;
            taskToUpdate.ModifiedBy = modifiedById;

            await _unitOfWork.Tasks.UpdateAsync(taskToUpdate); // Cập nhật nhiệm vụ vào DbSet
            await _unitOfWork.CommitAsync(); // Lưu thay đổi vào cơ sở dữ liệu
        }

        
    }

    

}
