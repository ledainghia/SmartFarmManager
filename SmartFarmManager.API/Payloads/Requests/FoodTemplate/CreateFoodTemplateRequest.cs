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

        public decimal? RecommendedWeightPerDay { get; set; } // Trọng lượng khuyến nghị mỗi ngày
        [SessionValidator]
        public int Session { get; set; } // Buổi (Sáng/Chiều/Tối)
        public decimal? WeightBasedOnBodyMass { get; set; } // Trọng lượng dựa trên khối lượng cơ thể

        public CreateFoodTemplateModel MapToModel()
        {
            return new CreateFoodTemplateModel
            {
                StageTemplateId = this.StageTemplateId,
                FoodName = this.FoodName,
                RecommendedWeightPerDay = this.RecommendedWeightPerDay,
                Session = this.Session,
                WeightBasedOnBodyMass = this.WeightBasedOnBodyMass
            };
        }
    }
}
