using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Dashboard
{
    public class FarmingBatchStatisticModel
    {
        public int TotalFarmingBatches { get; set; } // Tổng số vụ nuôi
        public int PlanningFarmingBatches { get; set; } // Số vụ nuôi đang lên kế hoạch
        public int ActiveFarmingBatches { get; set; } // Số vụ nuôi đang hoạt động
        public int CompletedFarmingBatches { get; set; } // Số vụ nuôi đã hoàn thành
        public int CancelledFarmingBatches { get; set; } // Số vụ nuôi bị hủy
    }
}
