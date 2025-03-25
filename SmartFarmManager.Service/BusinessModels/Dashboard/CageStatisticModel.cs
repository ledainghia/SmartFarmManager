using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Dashboard
{
    public class CageStatisticModel
    {
        public int TotalCages { get; set; } // Tổng số chuồng
        public int NormalCages { get; set; } // Số chuồng đang hoạt động
        public int EmptyCages { get; set; } // Số chuồng chưa có vụ nuôi đang hoạt động
        public int CagesWithActiveFarmingBatches { get; set; } // Số chuồng có vụ nuôi đang hoạt động
        public int IsolatedCages { get; set; } // Số chuồng đang cách ly
    }
}
