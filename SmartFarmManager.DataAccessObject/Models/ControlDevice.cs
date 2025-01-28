using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public partial class ControlDevice : EntityBase
    {
        public Guid Id { get; set; } // ID thiết bị
        public string Name { get; set; } // Tên thiết bị
        public string Type { get; set; } // Loại thiết bị
        public string ControlCode { get; set; } // Lệnh điều khiển
        public string Command { get; set; } // Mã lệnh hoặc comment
        public DateTime CreatedAt { get; set; } // Ngày tạo
        public Guid CageId { get; set; } // FK với Cage
        public virtual Cage Cage { get; set; } // Navigation property
        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
