using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Sensor
{
    public class UpdateSensorModel
    {
        public Guid? SensorTypeId { get; set; }
        public Guid? CageId { get; set; }
        public string? Name { get; set; }
        public int? PinCode { get; set; }
        public bool? Status { get; set; }
        public int? NodeId { get; set; }
    }
}

