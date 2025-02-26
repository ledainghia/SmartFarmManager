using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.FoodStack
{
    public class FoodStackItemModel
    {
        public Guid Id { get; set; }
        public Guid FarmId { get; set; }
        public string FoodType { get; set; }
        public decimal Quantity { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal CostPerKg { get; set; }
    }

}
