using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public partial class CostingReport : EntityBase
    {
        public Guid Id { get; set; } // Khóa chính

        public Guid FarmId { get; set; } // Khóa ngoại tham chiếu đến Farm

        public int ReportMonth { get; set; } // Tháng (1-12)

        public int ReportYear { get; set; } // Năm

        public string CostType { get; set; } // Loại chi phí (Điện, Nước, Nhân công, v.v.)

        public decimal TotalQuantity { get; set; } // Tổng số lượng

        public decimal TotalCost { get; set; } // Tổng chi phí

        public DateTime GeneratedAt { get; set; } // Ngày tạo báo cáo

        public virtual Farm Farm { get; set; } // Navigation property đến Farm
    }

}
