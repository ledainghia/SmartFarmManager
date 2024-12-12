using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.TaskType;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class TaskTypeService : ITaskTypeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaskTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> CreateTaskTypeAsync(TaskTypeModel taskTypeModel)
        {
            var taskType = new TaskType
            {
                TaskTypeName = taskTypeModel.TaskTypeName,
                PriorityNum = taskTypeModel.PriorityNum
            };

            await _unitOfWork.TaskTypes.CreateAsync(taskType);
            await _unitOfWork.CommitAsync();
            return taskType.Id;
        }

        public async Task<IEnumerable<TaskTypeModel>> GetTaskTypesAsync()
        {
            var taskTypes = await _unitOfWork.TaskTypes.FindAll().ToListAsync();
            return taskTypes.Select(tt => new TaskTypeModel
            {
                Id = tt.Id,
                TaskTypeName = tt.TaskTypeName,
                PriorityNum = tt.PriorityNum
            });
        }

        public async Task<TaskTypeModel> GetTaskTypeByIdAsync(Guid id)
        {
            var taskType = await _unitOfWork.TaskTypes.GetByIdAsync(id);
            if (taskType == null) return null;

            return new TaskTypeModel
            {
                Id = taskType.Id,
                TaskTypeName = taskType.TaskTypeName,
                PriorityNum = taskType.PriorityNum
            };
        }

        public async Task<bool> UpdateTaskTypeAsync(TaskTypeModel taskTypeModel)
        {
            var taskType = await _unitOfWork.TaskTypes.GetByIdAsync(taskTypeModel.Id);
            if (taskType == null) return false;

            taskType.TaskTypeName = taskTypeModel.TaskTypeName;
            taskType.PriorityNum = taskTypeModel.PriorityNum;

            await _unitOfWork.TaskTypes.UpdateAsync(taskType);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteTaskTypeAsync(Guid id)
        {
            var taskType = await _unitOfWork.TaskTypes.GetByIdAsync(id);
            if (taskType == null) return false;

            await _unitOfWork.TaskTypes.DeleteAsync(taskType);
            await _unitOfWork.CommitAsync();
            return true;
        }
    }

}
