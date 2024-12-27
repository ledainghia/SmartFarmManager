namespace SmartFarmManager.API.Payloads.Responses.VaccineScheduleLog
{
    public class VaccineScheduleLogResponse
    {
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public DateOnly? Date { get; set; }
        public string Notes { get; set; }
        public string Photo { get; set; }
        public Guid? TaskId { get; set; }
    }
}
