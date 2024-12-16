using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.FoodTemplate
{
    public class FoodTemplateFilterPagingRequest
    {
        public Guid? StageTemplateId { get; set; } // Lọc theo GrowthStageTemplateId
        public string? FoodName { get; set; } // Tìm kiếm theo tên thực phẩm
        public int? Session { get; set; } // Lọc theo buổi (sáng/chiều/tối)

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than 0.")]
        public int PageNumber { get; set; } = 1;

        [Required]
        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 10;
    }
}
