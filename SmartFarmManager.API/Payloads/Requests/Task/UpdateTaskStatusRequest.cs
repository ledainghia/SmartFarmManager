using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.Task
{
    public class UpdateTaskStatusRequest
    {
        [Required]
        [RegularExpression(@"^To Do|In Progress|Verifying|Done|Cancel$", ErrorMessage = "Invalid status.")]
        public string Status { get; set; } = null!;
    }
}
