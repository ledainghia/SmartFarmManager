using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Webhook
{
    public class SensorDataRecord
    {
        // Thời gian ghi nhận dữ liệu cảm biến (thường là thời điểm gửi hoặc đo đạc)
        public DateTime Time { get; set; }

        // Giá trị của cảm biến (ví dụ: nhiệt độ, độ ẩm, mức độ H2S)
        public double Value { get; set; }

        // Constructor mặc định
        public SensorDataRecord() { }

        // Constructor với tham số
        public SensorDataRecord(DateTime time, double value)
        {
            Time = time;
            Value = value;
        }
    }
}
