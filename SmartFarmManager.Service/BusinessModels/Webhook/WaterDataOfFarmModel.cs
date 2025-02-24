using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Webhook
{
    public class WaterDataOfFarmModel
    {
        public string FarmCode { get; set; }
        public List<WaterRecordModel> Data { get; set; }  // Danh sách các record nước
        public DateTime CreatedDate { get; set; }  // Ngày tạo bản ghi
    }

    public class WaterRecordModel
    {
        public DateTime BeginTime { get; set; }  // Thời gian bắt đầu
        public DateTime EndTime { get; set; }    // Thời gian kết thúc
        public double? Value { get; set; }       // Giá trị nước tiêu thụ (m³)
        public DateTime Date { get; set; }       // Ngày và giờ ghi nhận dữ liệu
    }
}
