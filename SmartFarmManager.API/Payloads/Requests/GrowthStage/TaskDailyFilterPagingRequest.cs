using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.GrowthStage
{
    public class TaskDailyFilterPagingRequest
    {
        public string? TaskName { get; set; } // Lọc theo tên Task
        public int? Session { get; set; } // Lọc theo buổi (Session)
        public int? PageNumber { get; set; } = 1; // Số trang
        public int? PageSize { get; set; } = 10; // Số phần tử mỗi trang
    }
}
