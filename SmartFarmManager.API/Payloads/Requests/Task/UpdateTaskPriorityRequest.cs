using SmartFarmManager.Service.BusinessModels.Task;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.Task
{
    public class UpdateTaskPriorityRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Priority must be greater than 0.")]
        public int NewPriority { get; set; }

        [Required]
        public int Session { get; set; } 

        public UpdateTaskPriorityModel MapToModel()
        {
            return new UpdateTaskPriorityModel {
            NewPriority=this.NewPriority,
            Session=this.Session         
            };
        }
    }
}
