using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.TaskDailyTemplate
{
    public class TaskDailyTemplateFilterPagingRequest
    {
        public Guid? GrowthStageTemplateId { get; set; } // Lọc theo GrowthStageTemplateId
        public string? TaskName { get; set; } // Lọc theo tên Task
        public int? Session { get; set; } // Lọc theo session

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than 0.")]
        public int PageNumber { get; set; } = 1;

        [Required]
        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 10;
    }

}
