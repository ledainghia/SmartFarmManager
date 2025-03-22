using SmartFarmManager.Service.BusinessModels.DailyFoodUsageLog;
using SmartFarmManager.Service.BusinessModels.Prescription;
using SmartFarmManager.Service.BusinessModels.Vaccine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.GrowthStage
{
    public class GrowthStageReportModel
    {
        public Guid StageId { get; set; }
        public string StageName { get; set; }
        public DateTime? AgeStartDate { get; set; }
        public DateTime? AgeEndDate { get; set; }
        public decimal? WeightAnimal { get; set; }
        public decimal? WeightAnimalExpect { get; set; }

        public int? Quantity { get; set; }
        public int? DeadQuantity { get; set; }

        public List<VaccineDetail> Vaccines { get; set; }
        public List<FoodUsageDetail> Foods { get; set; }
        public List<PrescriptionDetail> Prescriptions { get; set; }
    }

}
