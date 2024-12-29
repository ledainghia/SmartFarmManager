namespace SmartFarmManager.API.Payloads.Requests.VaccineScheduleLog
{
    public class CreateVaccineScheduleLogRequest
    {
        public DateOnly? Date { get; set; }
        public string Notes { get; set; }
        public string Photo { get; set; }
        public Guid? TaskId { get; set; }
    }
}
