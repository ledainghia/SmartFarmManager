using SmartFarmManager.API.Controllers;

namespace SmartFarmManager.API.Payloads.Requests.Task
{
    public class TaskDetailResponse
    {
        public int Id { get; set; }
        public string TaskName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public string TaskType { get; set; } = null!;
        public int? FarmId { get; set; }
        public int? AssignedToUserId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CompleteAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public List<TaskHistoryResponse> TaskHistories { get; set; } = new List<TaskHistoryResponse>();
    }
}
