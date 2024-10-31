namespace SmartFarmManager.API.Payloads.Requests.Task
{
    public class UpdateTaskRequest
    {
        public string? TaskName { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string? TaskType { get; set; }
        public int? FarmId { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? Status { get; set; }
    }
}
