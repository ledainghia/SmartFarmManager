using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Webhook
{
    public class AlertSensorOfFarmModel
    {
        public string FarmCode { get; set; }  // Mã farm
        public string PenCode { get; set; }   // Mã chuồng
        public int NodeId { get; set; } // Ma node
        public int PinCode { get; set; }      // Mã sensor (pinCode)
        public double Value { get; set; }     // Giá trị đo được
        public double Threshold { get; set; } // Ngưỡng sensor
        public string AlertType { get; set; } // Kiểu cảnh báo (AboveThreshold/BelowThreshold)
        public string Message { get; set; }   // Thông báo cảnh báo
        public DateTime CreatedDate { get; set; }  // Thời gian tạo cảnh báo
    }

}
