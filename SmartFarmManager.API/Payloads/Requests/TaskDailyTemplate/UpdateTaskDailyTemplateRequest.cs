using SmartFarmManager.API.Validation;
using SmartFarmManager.Service.BusinessModels.TaskDailyTemplate;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.TaskDailyTemplate
{
    public class UpdateTaskDailyTemplateRequest
    {
        public Guid? TaskTypeId { get; set; }

        [MaxLength(50)]
        public string? TaskName { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [ValidSessionType]
        public int? Session { get; set; }

        public UpdateTaskDailyTemplateModel MapToModel(Guid id)
        {
            return new UpdateTaskDailyTemplateModel
            {
                Id = id,
                TaskTypeId = this.TaskTypeId,
                TaskName = this.TaskName,
                Description = this.Description,
                Session = this.Session
            };
        }
    }
}
