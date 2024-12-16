using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.FoodTemplate
{
    public class FoodTemplateDetailModel
    {
        public Guid Id { get; set; }
        public Guid StageTemplateId { get; set; }
        public string FoodName { get; set; }
        public decimal? RecommendedWeightPerDay { get; set; }
        public int Session { get; set; }
        public decimal? WeightBasedOnBodyMass { get; set; }

        // Thông tin StageTemplate liên kết
        public GrowthStageTemplateResponse? GrowthStageTemplate { get; set; }
    }

    public class GrowthStageTemplateResponse
    {
        public Guid Id { get; set; }
        public string StageName { get; set; }
        public decimal? WeightAnimal { get; set; }
        public int? AgeStart { get; set; }
        public int? AgeEnd { get; set; }
        public string? Notes { get; set; }
    }
}
