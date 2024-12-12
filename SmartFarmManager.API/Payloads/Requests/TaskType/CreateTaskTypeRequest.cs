namespace SmartFarmManager.API.Payloads.Requests.TaskType
{
    public class CreateTaskTypeRequest
    {
        public string TaskTypeName { get; set; }
        public int? PriorityNum { get; set; }
    }
}
