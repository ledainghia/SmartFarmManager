namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class TaskResponse
    {
        public Guid TaskId { get; set; }
        public string TaskName { get; set; }
        public string Cagename { get; set; }
        public string AssignName { get; set; }
        public int PriorityNum { get; set; }
        public bool IsDisabled { get; set; } // Task có bị làm mờ không
        public string Status { get; set; }
        public DateTime? DueDate { get; set; }
        public string Reason { get; set; } // Lý do (nếu bị làm mờ)
    }
}
