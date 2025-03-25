using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Dashboard
{
    public class VaccineScheduleStatisticModel
    {
        public int TotalVaccineSchedules { get; set; } // Tổng số lịch vaccine
        public int UpcomingVaccineSchedules { get; set; } // Số lịch vaccine sắp tới
        public int CompletedVaccineSchedules { get; set; } // Số lịch vaccine đã tiêm
        public int MissedVaccineSchedules { get; set; } // Số lịch vaccine bỏ lỡ
        public int CancelledVacineSchedules { get; set; } // Số lịch vaccine bị hủy
        public int RedoVaccineSchedules { get; set; } // Số lịch vaccine cần tiêm lại
    }
}
