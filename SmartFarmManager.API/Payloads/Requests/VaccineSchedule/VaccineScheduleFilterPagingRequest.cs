namespace SmartFarmManager.API.Payloads.Requests.VaccineSchedule
{
    public class VaccineScheduleFilterPagingRequest
    {
        public string? KeySearch { get; set; }
        public Guid? VaccineId { get; set; }
        public Guid? StageId { get; set; }
        public DateTime? Date { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
