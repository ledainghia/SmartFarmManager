using SmartFarmManager.API.Validation;
using SmartFarmManager.Service.BusinessModels.Task;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.Task
{
    public class CreateTaskRequest
    {
        [Required]
        public Guid? TaskTypeId { get; set; }

        [Required]
        public Guid CageId { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string TaskName { get; set; }


        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
        [Required]
        [SessionValidator]
        public string Session { get; set; }
        public  CreateTaskModel MapToModel()
        {
            return new CreateTaskModel
            {
                TaskTypeId = this.TaskTypeId,
                CageId = this.CageId,
                CreatedByUserId = this.CreatedByUserId,
                TaskName = this.TaskName,
                Description =this.Description,
                DueDate = this.DueDate,
                Session = this.Session
            };
        }
    }
}
