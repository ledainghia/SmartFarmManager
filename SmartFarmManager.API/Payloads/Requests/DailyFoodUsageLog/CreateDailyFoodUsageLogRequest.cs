namespace SmartFarmManager.API.Payloads.Requests.DailyFoodUsageLog
{
    public class CreateDailyFoodUsageLogRequest
    {
        public Guid StageId { get; set; }
        public decimal? RecommendedWeight { get; set; }
        public decimal? ActualWeight { get; set; }
        public string Notes { get; set; }
        public DateTime? LogTime { get; set; }
        public string Photo { get; set; }
        public Guid TaskId { get; set; }
    }
}
