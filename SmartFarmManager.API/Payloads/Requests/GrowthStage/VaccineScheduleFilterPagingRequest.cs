using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.GrowthStage
{
    public class VaccineScheduleFilterPagingRequest
    {
        public string? Status { get; set; } // Lọc theo trạng thái (Upcoming, Completed, Missed)

        public int? PageNumber { get; set; } = 1; // Số trang

        public int? PageSize { get; set; } = 10; // Số phần tử mỗi trang
    }
}
