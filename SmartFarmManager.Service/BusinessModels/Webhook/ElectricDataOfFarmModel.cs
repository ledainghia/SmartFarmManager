using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Webhook
{
    public class ElectricDataOfFarmModel
    {
        public string FarmCode { get; set; }
        public List<ElectricRecordModel> Data { get; set; }  // Danh sách các record điện
        public DateTime CreatedDate { get; set; }  // Ngày tạo bản ghi
    }

    public class ElectricRecordModel
    {
        public DateTime BeginTime { get; set; }  // Thời gian bắt đầu của mỗi giờ
        public DateTime EndTime { get; set; }    // Thời gian kết thúc của mỗi giờ
        public double Value { get; set; }        // Giá trị điện tiêu thụ (kWh)
        public DateTime Date { get; set; }       // Ngày và giờ ghi nhận dữ liệu
    }
}
