using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Dashboard
{
    public class TaskStatisticModel
    {
        public int TotalTasks { get; set; } // Tổng số công việc
        public int PendingTasks { get; set; } // Công việc đang chờ
        public int InProgressTasks { get; set; } // Công việc đang thực hiện
        public int DoneTasks { get; set; } // Công việc đã hoàn thành
        public int OverdueTasks { get; set; } // Công việc bị trễ hạn
        public int CancelledTasks { get; set; } // Công việc bị hủy
    }
}
