using SmartFarmManager.API.Validation;
using SmartFarmManager.Service.BusinessModels.FoodTemplate;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.FoodTemplate
{
    public class CreateFoodTemplateRequest
    {
        [Required]
        public Guid StageTemplateId { get; set; } 

        [Required]
        [MaxLength(100)]
        public string FoodName { get; set; } // Tên của thực phẩm

        public decimal? RecommendedWeightPerSession { get; set; } // Trọng lượng khuyến nghị mỗi ngày
        public decimal? WeightBasedOnBodyMass { get; set; } // Trọng lượng dựa trên khối lượng cơ thể

        public CreateFoodTemplateModel MapToModel()
        {
            return new CreateFoodTemplateModel
            {
                StageTemplateId = this.StageTemplateId,
                FoodName = this.FoodName,
                RecommendedWeightPerSession = this.RecommendedWeightPerSession,
                WeightBasedOnBodyMass = this.WeightBasedOnBodyMass
            };
        }
    }
}
