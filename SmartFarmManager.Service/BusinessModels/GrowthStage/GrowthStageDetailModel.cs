using SmartFarmManager.Service.BusinessModels.TaskDaily;
using SmartFarmManager.Service.BusinessModels.VaccineSchedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.GrowthStage
{
    public class GrowthStageDetailModel
    {
        public Guid Id { get; set; }
        public Guid FarmingBatchId { get; set; }
        public string Name { get; set; }
        public decimal? WeightAnimal { get; set; }
        public int? Quantity { get; set; }
        public int? AgeStart { get; set; }
        public int? AgeEnd { get; set; }
        public string FoodType { get;set; }
        public DateTime? AgeStartDate { get; set; }
        public DateTime? AgeEndDate { get; set; }
        public string Status { get; set; }
        public int? AffectQuantity { get; set; }
        public int? DeadQuantity { get; set; }

        public decimal? RecommendedWeightPerSession { get; set; }
        public decimal? WeightBasedOnBodyMass { get; set; }
        public List<TaskDailyModel> TaskDailies { get; set; }
        public List<VaccineScheduleModel> VaccineSchedules { get; set; }
    }
}
