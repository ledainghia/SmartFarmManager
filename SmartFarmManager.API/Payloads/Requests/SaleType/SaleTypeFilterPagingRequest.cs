using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.SaleType
{
    public class SaleTypeFilterPagingRequest
    {
        public string? StageTypeName { get; set; } // Lọc theo tên giai đoạn bán
        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than or equal to 1.")]
        public int PageNumber { get; set; } = 1; // Số trang
        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 10; // Số phần tử mỗi trang
    }

}
