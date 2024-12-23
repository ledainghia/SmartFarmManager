using SmartFarmManager.API.Validation;
using SmartFarmManager.Service.BusinessModels.Task;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.Task
{
    public class TaskDetailUpdateRequest
    {
        [MaxLength(200)]
        public string TaskName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public Guid? TaskTypeId { get; set; }


        public DateTime? DueDate { get; set; }

        [SessionValidator]
        public string Session { get; set; }

        public TaskDetailUpdateModel MapToModel(Guid taskId)
        {
            return new TaskDetailUpdateModel
            {
                TaskId = taskId,
                TaskName = this.TaskName,
                Description = this.Description,
                TaskTypeId = this.TaskTypeId,
                DueDate = this.DueDate,
                Session = this.Session
            };
        }
    }
}
