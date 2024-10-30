using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository;
using SmartFarmManager.Repository.Interfaces;
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
    }

    

}
