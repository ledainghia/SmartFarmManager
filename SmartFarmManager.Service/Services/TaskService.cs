using SmartFarmManager.DataAccessObject.Models;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.Interfaces;
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
        public async Task<bool> CreateTaskAsync(CreateTaskModel model)
        {
            if (model.DueDate < DateTime.UtcNow)
            {
                throw new ArgumentException("DueDate must be in the future");
            }

            var taskType = await _unitOfWork.TaskTypes.FindAsync(x=>x.Id==model.TaskTypeId);
            if (taskType == null)
            {
                throw new ArgumentException("Invalid TaskTypeId");
            }


            var task = new DataAccessObject.Models.Task
            {
                TaskTypeId = model.TaskTypeId,
                CageId = model.CageId,
                AssignedToUserId = model.AssignedToUserId,
                CreatedByUserId = model.CreatedByUserId,
                TaskName = model.TaskName,
                PriorityNum = (int)taskType.PriorityNum,
                Description = model.Description,
                DueDate = model.DueDate,
                Session = model.Session,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending" 
            };

            await _unitOfWork.Tasks.CreateAsync(task);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> UpdateTaskPriorityAsync(Guid taskId, UpdateTaskPriorityModel model)
        {
            
            var task = await _unitOfWork.Tasks.FindAsync(x=>x.Id==taskId);
            if (task == null)
            {
                throw new KeyNotFoundException("Task not found.");
            }

            if (task.Session != model.Session)
            {
                var taskDate = task.DueDate?.Date;
                if (taskDate == null)
                {
                    throw new ArgumentException("Task does not have a valid DueDate.");
                }

                // Lấy tất cả task trong cùng ngày chung session khác (cùng ngày, chung session)
                var tasksInOldSession = await _unitOfWork.Tasks
                        .FindByCondition(t => t.DueDate.HasValue &&
                                  t.DueDate.Value.Date == taskDate.Value.Date &&
                                  t.Session == task.Session &&
                                  t.CageId == task.CageId &&
                                  t.AssignedToUserId == task.AssignedToUserId)
                        .OrderBy(t => t.PriorityNum)
                        .ToListAsync();
                // Lấy tất cả task trong cùng ngày nhưng có session khác (cùng ngày, khác session)
                var tasksInNewSession = await _unitOfWork.Tasks
                        .FindByCondition(t => t.DueDate.HasValue &&
                                  t.DueDate.Value.Date == taskDate.Value.Date &&
                                  t.Session == model.Session &&
                                  t.CageId == task.CageId&&
                                  t.AssignedToUserId==task.AssignedToUserId)
                        .OrderBy(t => t.PriorityNum)
                        .ToListAsync();

                // 1. Cập nhật PriorityNum của các task trong session cũ
                foreach (var oldTask in tasksInOldSession)
                {
                    if (oldTask.PriorityNum > task.PriorityNum)
                    {
                        oldTask.PriorityNum -= 1;
                    }
                }

                // 2. Cập nhật PriorityNum của các task trong session mới
                foreach (var newTask in tasksInNewSession)
                {
                    if (newTask.PriorityNum >= model.NewPriority)
                    {
                        newTask.PriorityNum += 1;
                    }
                }
                task.Session = model.Session;
                task.PriorityNum = model.NewPriority;
                await _unitOfWork.Tasks.UpdateAsync(task);
                await _unitOfWork.CommitAsync();
                return true;



            }
            else
            {
                var taskDate = task.DueDate?.Date;
                if (taskDate == null)
                {
                    throw new ArgumentException("Task does not have a valid DueDate.");
                }

                // Lấy tất cả các task trong cùng ngày và cùng session
                var tasksInSameSession = await _unitOfWork.Tasks
                    .FindByCondition(t => t.DueDate.HasValue &&
                                          t.DueDate.Value.Date == taskDate.Value.Date &&
                                          t.Session == task.Session &&
                                          t.CageId == task.CageId &&
                                          t.AssignedToUserId == task.AssignedToUserId)
                    .OrderBy(t => t.PriorityNum)
                    .ToListAsync();

                // Giảm PriorityNum của các task có PriorityNum > PriorityNum hiện tại
                foreach (var t in tasksInSameSession.Where(t => t.PriorityNum > task.PriorityNum))
                {
                    t.PriorityNum -= 1;
                }

                // Tăng PriorityNum của các task có PriorityNum >= newPriority
                foreach (var t in tasksInSameSession.Where(t => t.PriorityNum >= model.NewPriority))
                {
                    t.PriorityNum += 1;
                }
                task.PriorityNum = model.NewPriority;

                await _unitOfWork.Tasks.UpdateAsync(task);    
                await _unitOfWork.CommitAsync();
                return true;

            }







        }


       
        }

    

        //change status of task by task id and status id
        public async Task<bool> ChangeTaskStatusAsync(int taskId, int statusId)
        {
            var task = await _unitOfWork.Tasks.FindAsync(x => x.Id.Equals(taskId));
            if (task == null)
            {
                throw new ArgumentException("Invalid TaskId");
            }

            var status = await _unitOfWork.Statuses.FindAsync(x => x.Id.Equals(statusId));
            if (status == null)
            {
                throw new ArgumentException("Invalid StatusId");
            }
            var statusLog = new StatusLog
            {
                TaskId = task.Id,
                StatusId = status.Id,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.StatusLogs.CreateAsync(statusLog);
            if (status.StatusName == "Hoàn thành")
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            task.Status = status.StatusName;
            await _unitOfWork.Tasks.UpdateAsync(task);
            await _unitOfWork.CommitAsync();

            return true;
        }
    }
}
