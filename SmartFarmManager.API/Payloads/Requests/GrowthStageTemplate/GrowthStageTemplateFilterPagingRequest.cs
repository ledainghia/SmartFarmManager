using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.GrowthStageTemplate
{
    public class GrowthStageTemplateFilterPagingRequest
    {
        public Guid? TemplateId { get; set; } // Lọc theo TemplateId
        public string? StageName { get; set; } // Tìm kiếm theo tên
        public int? AgeStart { get; set; } // Lọc theo tuổi bắt đầu
        public int? AgeEnd { get; set; } // Lọc theo tuổi kết thúc

        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than or equal to 1.")]
        public int PageNumber { get; set; } = 1; // Số trang

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 10; // Số phần tử mỗi trang
    }
}
