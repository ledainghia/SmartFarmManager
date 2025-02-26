using SmartFarmManager.Service.BusinessModels.FoodStack;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.FoodStack
{
    public class FoodStackCreateRequest
    {
        [Required]
        public Guid FarmId { get; set; }
        [Required]
        public string FoodType { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "CostPerKg must be greater than 0.")]
        public decimal CostPerKg { get; set; }

        public FoodStackCreateModel MapToModel()
        {
            return new FoodStackCreateModel
            {
                FarmId = this.FarmId,
                FoodType = this.FoodType,
                Quantity = this.Quantity,
                CostPerKg = this.CostPerKg
            };


        }
    }

}
