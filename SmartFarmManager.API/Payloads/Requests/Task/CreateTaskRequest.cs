using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.Task
{
    public class CreateTaskRequest
    {
        [Required(ErrorMessage = "TaskName is required.")]
        [StringLength(100, ErrorMessage = "TaskName can't be longer than 100 characters.")]
        public string TaskName { get; set; } = null!;
   
        public string? Description { get; set; }

        [Required(ErrorMessage = "DueDate is required.")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "TaskType is required.")]
        [RegularExpression(@"^System|Farm$", ErrorMessage = "TaskType must be either 'System' or 'Farm'.")]
        public string TaskType { get; set; } = null!;

        public int? FarmId { get; set; } 

        public int? AssignedToUserId { get; set; }
    }
}
