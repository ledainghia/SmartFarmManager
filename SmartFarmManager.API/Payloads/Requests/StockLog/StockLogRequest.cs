using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.BusinessModels.StockLog;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.StockLog
{
    public class StockLogRequest
    {
        [Required]
        public Guid FarmId { get; set; }
        [Required]
        [MaxLength(50)]
        public string FoodType { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "CostPerKg must be greater than 0.")]
        public decimal CostPerKg { get; set; }
        public StockLogRequestModel MapToModel()
        {
            return new StockLogRequestModel
            {
                FarmId = this.FarmId,
                FoodType = this.FoodType,
                Quantity = this.Quantity,
                CostPerKg = this.CostPerKg
            };
        }
    }
    

}
