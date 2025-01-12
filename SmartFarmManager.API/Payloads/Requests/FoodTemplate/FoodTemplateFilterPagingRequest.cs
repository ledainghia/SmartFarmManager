using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.FoodTemplate
{
    public class FoodTemplateFilterPagingRequest
    {
        public Guid? StageTemplateId { get; set; } // Lọc theo GrowthStageTemplateId
        public string? FoodType { get; set; } 

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
