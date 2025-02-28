using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Shared
{
    public static class TaskStatusEnum
    {
        public const string Pending = "Pending"; // Công việc đang chờ xử lý
        public const string InProgress = "InProgress"; // Công việc đang được thực hiện
        public const string Done = "Done"; // Công việc đã hoàn thành
        public const string Overdue = "Overdue"; // Công việc bị trễ hạn, chưa hoàn thành trước deadline
        public const string Cancelled = "Cancelled"; // Công việc đã bị hủy
    }
}
