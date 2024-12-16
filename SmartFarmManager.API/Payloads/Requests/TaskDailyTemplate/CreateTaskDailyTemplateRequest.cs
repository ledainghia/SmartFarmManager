using SmartFarmManager.API.Validation;
using SmartFarmManager.Service.BusinessModels.TaskDailyTemplate;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.TaskDailyTemplate
{
    public class CreateTaskDailyTemplateRequest
    {
        [Required]
        public Guid GrowthStageTemplateId { get; set; }

        public Guid? TaskTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TaskName { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        [ValidSessionType]
        public int Session { get; set; }

        public CreateTaskDailyTemplateModel MapToModel()
        {
            return new CreateTaskDailyTemplateModel
            {
                GrowthStageTemplateId = this.GrowthStageTemplateId,
                TaskTypeId = this.TaskTypeId,
                TaskName = this.TaskName,
                Description = this.Description,
                Session = this.Session
            };
        }
    }
}
