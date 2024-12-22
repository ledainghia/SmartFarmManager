using SmartFarmManager.Service.BusinessModels.Task;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.Task
{
    public class CreateTaskRecurringRequest
    {
        [Required]
        public Guid CageId { get; set; }

        [Required]
        public Guid TaskTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TaskName { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        [MinLength(1)] 
        public List<int> Sessions { get; set; } 

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        public CreateTaskRecurringModel MapToModel()
        {
            return new CreateTaskRecurringModel
            {
                CageId = this.CageId,
                TaskTypeId = this.TaskTypeId,
                TaskName = this.TaskName,
                Description = this.Description,
                Sessions = this.Sessions,
                StartAt = this.StartAt,
                EndAt = this.EndAt
            };
        }
    }
}
