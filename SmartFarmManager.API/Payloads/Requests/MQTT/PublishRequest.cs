namespace SmartFarmManager.API.Payloads.Requests.MQTT
{
    public class PublishRequest
    {
        public string Topic { get; set; }
        public string Payload { get; set; }
    }
}
