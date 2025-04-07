using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Sensor
{
    public class SensorFilterModel
    {
        public string? KeySearch { get; set; }
        public Guid FarmId { get; set; }
        public string? Status { get; set; }
        public Guid? SensorTypeId { get; set; }
        public int? NodeId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
