using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.SensorDataLog
{
    public class SensorRecordModel
    {
        public DateTime BeginTime { get; set; }  // Thời gian bắt đầu
        public DateTime EndTime { get; set; }    // Thời gian kết thúc
        public double Value { get; set; }        // Giá trị sensor (giá trị đo được từ sensor)
        public DateTime Date { get; set; }       // Ngày ghi nhận
    }

}
