using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.FoodTemplate
{
    public class CreateFoodTemplateModel
    {
        public Guid StageTemplateId { get; set; }
        public string FoodName { get; set; }
        public decimal? RecommendedWeightPerSession { get; set; }
        public decimal? WeightBasedOnBodyMass { get; set; }
    }
}
