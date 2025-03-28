using SmartFarmManager.Service.BusinessModels.FoodStack;

namespace SmartFarmManager.API.Payloads.Requests.FoodStack
{
    public class UpdateFoodStackRequest
    {
        public string FoodType { get; set; }
        public UpdateFoodStockModel MapToModel()
        {
            return new UpdateFoodStockModel
            {
                FoodType = this.FoodType,              
            };
        }
    }
}
