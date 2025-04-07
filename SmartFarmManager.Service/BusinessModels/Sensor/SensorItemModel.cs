using SmartFarmManager.DataAccessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Sensor
{
    public class SensorItemModel
    {
        public Guid Id { get; set; }
        public Guid SensorTypeId { get; set; }
        public string SensorTypeName { get; set; }

        public Guid CageId { get; set; }
        public string CageName { get; set; }

        public string SensorCode { get; set; }

        public string Name { get; set; }

        public int PinCode { get; set; }

        public bool Status { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedDate { get; set; }

        public int NodeId { get; set; }

    }
}
