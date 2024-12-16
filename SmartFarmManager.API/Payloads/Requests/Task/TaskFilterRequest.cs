using SmartFarmManager.Service.BusinessModels.Task;

namespace SmartFarmManager.API.Payloads.Requests.Task
{
    public class TaskFilterRequest
    {
        public Guid? TaskTypeId { get; set; }
        public Guid? CageId { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public string? TaskName { get; set; }
        public int? PriorityNum { get; set; } 
        public string? Description { get; set; } 
        public DateTime? DueDate { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int? Session { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }

        public TaskModel MapToModel()
        {
            return new TaskModel
            {
                TaskTypeId = TaskTypeId,
                CageId = CageId,
                AssignedToUserId = AssignedToUserId,
                CreatedByUserId = CreatedByUserId,
                TaskName = TaskName,
                PriorityNum = PriorityNum,
                Description = Description,
                DueDate = DueDate,
                Status = Status,
                Session = Session,
                CompletedAt = CompletedAt,
                CreatedAt = CreatedAt
            };
        }
    }
}
