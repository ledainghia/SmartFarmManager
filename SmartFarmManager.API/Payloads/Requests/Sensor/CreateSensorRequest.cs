namespace SmartFarmManager.API.Payloads.Requests.Sensor
{
    public class CreateSensorRequest
    {
        public Guid SensorTypeId { get; set; }
        public Guid CageId { get; set; }
        public string SensorCode { get; set; }
        public string Name { get; set; }
        public int PinCode { get; set; }
        public int NodeId { get; set; }
    }
}
