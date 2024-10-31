namespace SmartFarmManager.API.Controllers
{
    public class TaskHistoryResponse
    {
        public int Id { get; set; }
        public string? StatusBefore { get; set; }
        public string StatusAfter { get; set; } = null!;
        public DateTime ChangedAt { get; set; }
    }
}
