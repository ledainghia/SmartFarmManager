using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public partial class MasterData : EntityBase
    {
        public Guid Id { get; set; } // Khóa chính

        public string CostType { get; set; } // Loại chi phí (Điện, Nước, Nhân công, v.v.)

        public string Unit { get; set; } // Đơn vị (kg, khối, ngày, v.v.)

        public decimal UnitPrice { get; set; } // Đơn giá (giá trên mỗi đơn vị)

        public Guid FarmId { get; set; } // Khóa ngoại tham chiếu đến Farm

        public virtual Farm Farm { get; set; } // Navigation property đến Farm
    }
}
