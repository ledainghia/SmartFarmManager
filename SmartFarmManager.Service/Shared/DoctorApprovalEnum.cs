using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Shared
{
    public class DoctorApprovalEnum
    {
        public const string Pending = "Pending"; // Chưa kiểm tra
        public const string Checked = "Checked"; // Đã kiểm tra
        public const string Merged = "Merged"; // Được gộp
        public const string NotMerged = "Not Merged"; // Không được gộp
    }
}
