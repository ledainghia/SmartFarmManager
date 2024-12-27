namespace SmartFarmManager.API.Payloads.Responses.DailyFoodUsageLog
{
    public class DailyFoodUsageLogResponse
    {
        public Guid Id { get; set; }
        public Guid StageId { get; set; }
        public decimal? RecommendedWeight { get; set; }
        public decimal? ActualWeight { get; set; }
        public string Notes { get; set; }
        public DateTime? LogTime { get; set; }
        public string Photo { get; set; }
        public Guid? TaskId { get; set; }
    }
}
