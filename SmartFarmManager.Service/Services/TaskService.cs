using SmartFarmManager.DataAccessObject.Models;
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
