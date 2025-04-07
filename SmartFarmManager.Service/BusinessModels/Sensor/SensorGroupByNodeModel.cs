using SmartFarmManager.Service.BusinessModels.Webhook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Sensor
{
    public class SensorGroupByNodeModel
    {
        public int NodeId { get; set; }  // NodeId để nhóm các sensor
        public List<SensorModel> Sensors { get; set; }  // Danh sách các sensor trong node đó
    }

}
