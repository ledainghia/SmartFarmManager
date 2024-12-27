using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.GrowthStage
{
    public class GrowthStageFilterPagingRequest
    {
        public Guid? FarmingBatchId { get; set; } // Lọc theo FarmingBatchId
        public string? Name { get; set; } // Tìm kiếm theo tên
        public int? AgeStart { get; set; } // Lọc theo tuổi bắt đầu
        public int? AgeEnd { get; set; } // Lọc theo tuổi kết thúc
        public string? Status { get; set; } // Lọc theo trạng thái (Pending, Active, Completed)

        public int? PageNumber { get; set; } = 1; // Số trang
        public int? PageSize { get; set; } = 10; // Số phần tử mỗi trang

        public string? OrderBy { get; set; } = "AgeStart"; // Sắp xếp theo trường (mặc định là AgeStart)
        public string? Order { get; set; } = "asc"; // Tăng dần ("asc") hoặc giảm dần ("desc")
    }

}
