namespace SmartFarmManager.API.Payloads.Requests.Sensor
{
    public class SensorFilterRequest
    {
        public string? KeySearch { get; set; }
        public Guid FarmId { get; set; }
        public string? Status { get; set; }
        public Guid? SensorTypeId { get; set; }
        public int? NodeId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
