using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.LogInTask
{
    public class DailyFoodUsageLogInTaskModel
    {
        public Guid GrowthStageId { get; set; } 
        public string? GrowthStageName { get; set; }
        public decimal? RecommendedWeight { get; set; } 
        public decimal? ActualWeight { get; set; }
        public string? Notes { get; set; }
        public string? Photo { get; set; }
        public DateTime LogTime { get; set; }
        public Guid? TaskId { get; set; }
        public double? UnitPrice { get; set; }
    }
}
