namespace SmartFarmManager.API.Payloads.Requests.VaccineScheduleLog
{
    public class CreateVaccineScheduleLogRequest
    {
        public Guid ScheduleId { get; set; }
        public DateOnly? Date { get; set; }
        public string Notes { get; set; }
        public string Photo { get; set; }
        public int? TaskId { get; set; }
    }
}
