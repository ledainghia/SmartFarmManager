using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Helpers
{
    public static class NotificationType
    {
        private static readonly Dictionary<string, string> NotificationTypeToVietnamese = new()
    {
        { "TaskDailyAssigned", "Nhiệm vụ hàng ngày đã được giao" },
        { "TaskOneTimeAssigned", "Nhiệm vụ một lần đã được giao" },
        { "UserAssignedToCage", "Người dùng được gán vào chuồng" },
        { "SymptomStatusUpdated", "Trạng thái triệu chứng đã được cập nhật" },
        { "SymptomReported", "Triệu chứng mới được báo cáo" },
        { "VaccineScheduleReminder", "Nhắc nhở lịch tiêm vắc-xin" },
        { "DailyReportReady", "Báo cáo hàng ngày đã sẵn sàng" },
        { "FarmBatchEnded", "Vụ nuôi đã kết thúc" },
        { "FoodRunningLow", "Thức ăn trong kho sắp hết" }
    };

        public static string GetVietnameseName(string notiTypeName)
        {
            if (NotificationTypeToVietnamese.TryGetValue(notiTypeName, out var vietnameseName))
            {
                return vietnameseName;
            }

            return "Loại thông báo không xác định"; // Trả về mặc định nếu không tìm thấy
        }
    }
}
