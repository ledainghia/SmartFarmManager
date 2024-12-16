namespace SmartFarmManager.API.Payloads.Requests.Task
{
    public class TaskPriorityUpdateRequest
    {
        public Guid TaskId { get; set; }
        public int PriorityNum { get; set; }
    }
}
