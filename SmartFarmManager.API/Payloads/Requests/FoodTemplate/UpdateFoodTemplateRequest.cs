using SmartFarmManager.API.Validation;
using SmartFarmManager.Service.BusinessModels.FoodTemplate;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.FoodTemplate
{
    public class UpdateFoodTemplateRequest
    {
        [MaxLength(100)]
        public string? FoodName { get; set; }

        public decimal? RecommendedWeightPerSession { get; set; }
        [ValidSessionType]
        public int? Session { get; set; }
        public decimal? WeightBasedOnBodyMass { get; set; }

        public UpdateFoodTemplateModel MapToModel(Guid id)
        {
            return new UpdateFoodTemplateModel
            {
                Id = id,
                FoodName = this.FoodName,
                RecommendedWeightPerSession = this.RecommendedWeightPerSession,
                Session = this.Session,
                WeightBasedOnBodyMass = this.WeightBasedOnBodyMass
            };
        }
    }
}
