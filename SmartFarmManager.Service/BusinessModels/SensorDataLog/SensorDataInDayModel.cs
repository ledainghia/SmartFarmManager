using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.SensorDataLog
{
    public class SensorDataInDayModel
    {
        public Guid SensorId { get; set; }  // Mã sensor
        public bool IsWarning { get; set; }  // Cảnh báo
        public List<SensorRecordModel> Data { get; set; }  // Danh sách dữ liệu sensor
    }

}
