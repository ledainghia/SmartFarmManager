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
        public string FoodType { get; set; } // Tên của thực phẩm

        public decimal? WeightBasedOnBodyMass { get; set; } // Trọng lượng dựa trên khối lượng cơ thể

        public CreateFoodTemplateModel MapToModel()
        {
            return new CreateFoodTemplateModel
            {
                StageTemplateId = this.StageTemplateId,
                FoodType = this.FoodType,
                WeightBasedOnBodyMass = this.WeightBasedOnBodyMass
            };
        }
    }
}
