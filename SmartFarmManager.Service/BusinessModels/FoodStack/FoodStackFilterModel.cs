using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.FoodStack
{
    public class FoodStackFilterModel
    {
        public Guid? FarmId { get; set; }
        public string? FoodType { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
