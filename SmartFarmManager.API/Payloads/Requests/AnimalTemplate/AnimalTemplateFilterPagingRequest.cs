using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.AnimalTemplate
{
    public class AnimalTemplateFilterPagingRequest
    {
        public string? Name { get; set; } // Tìm kiếm theo tên
        public string? Species { get; set; } // Lọc theo species
        public string? Status { get; set; } // Lọc theo trạng thái
        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than or equal to 1.")]
        public int PageNumber { get; set; } = 1; // Số trang
        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 10; // Số phần tử mỗi trang
    }
}
