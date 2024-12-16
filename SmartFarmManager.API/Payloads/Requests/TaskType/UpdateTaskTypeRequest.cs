namespace SmartFarmManager.API.Payloads.Requests.TaskType
{
    public class UpdateTaskTypeRequest
    {
        public string TaskTypeName { get; set; }
        public int? PriorityNum { get; set; }
    }
}
