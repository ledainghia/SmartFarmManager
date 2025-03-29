

using SmartFarmManager.Service.BusinessModels.Sensor;

namespace SmartFarmManager.API.Payloads.Requests.Sensor
{
    public class UpdateSensorRequest
    {
        public Guid? SensorTypeId { get; set; }
        public Guid? CageId { get; set; }
        public string? Name { get; set; }
        public int? PinCode { get; set; }
        public bool? Status { get; set; }
        public int? NodeId { get; set; }
        public UpdateSensorModel MapToModel()
        {
            return new UpdateSensorModel
            {
                SensorTypeId = this.SensorTypeId,
                CageId = this.CageId,
                Name = this.Name,
                PinCode = this.PinCode,
                Status = this.Status,
                NodeId = this.NodeId
            };
        }
    }
}
