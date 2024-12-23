using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Shared
{
    public static class GrowthStageStatusEnum
    {
        public const string Upcoming = "Upcoming";    // Chưa bắt đầu
        public const string Active = "Active";        // Đang trong giai đoạn
        public const string Completed = "Completed";  // Giai đoạn đã kết thúc
    }
}
