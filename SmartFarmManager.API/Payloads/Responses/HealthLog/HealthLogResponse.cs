namespace SmartFarmManager.API.Payloads.Responses.HealthLog
{
    public class HealthLogResponse
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public string Photo { get; set; }
        public Guid? TaskId { get; set; }
    }
}
