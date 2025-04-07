using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Dashboard
{
    public class DashboardStatisticsModel
    {
        public TaskStatisticModel TaskStatistics { get; set; } // Thống kê về các công việc
        public CageStatisticModel CageStatistics { get; set; } // Thống kê về các chuồng
        public FarmingBatchStatisticModel FarmingBatchStatistics { get; set; } // Thống kê về các vụ nuôi
        public StaffStatisticModel StaffStatistics { get; set; } // Thống kê về các nhân viên
        public VaccineScheduleStatisticModel VaccineScheduleStatistics { get; set; } // Thống kê về các lịch vaccine
    }
}
