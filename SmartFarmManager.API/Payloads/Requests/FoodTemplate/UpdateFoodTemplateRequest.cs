using SmartFarmManager.API.Validation;
using SmartFarmManager.Service.BusinessModels.FoodTemplate;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.FoodTemplate
{
    public class UpdateFoodTemplateRequest
    {
        [MaxLength(100)]
        public string? FoodType { get; set; }
        public decimal? WeightBasedOnBodyMass { get; set; }

        public UpdateFoodTemplateModel MapToModel(Guid id)
        {
            return new UpdateFoodTemplateModel
            {
                Id = id,
                FoodType = this.FoodType,
                WeightBasedOnBodyMass = this.WeightBasedOnBodyMass
            };
        }
    }
}
