using SmartFarmManager.Service.BusinessModels.DailyFoodUsageLog;
using SmartFarmManager.Service.BusinessModels.Prescription;
using SmartFarmManager.Service.BusinessModels.Vaccine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.FarmingBatch
{
    public class DetailedFarmingBatchReportResponse
    {
        public Guid FarmingBatchId { get; set; }
        public string FarmingBatchName { get; set; }
        public string CageName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal TotalEggSales { get; set; }
        public decimal TotalMeatSales { get; set; }
        public decimal TotalFoodCost { get; set; }
        public decimal TotalVaccineCost { get; set; }
        public decimal TotalMedicineCost { get; set; }
        public int TotalEggsCollected { get; set; }
        public decimal NetProfit { get; set; }
        public List<VaccineDetail> VaccineDetails { get; set; }
        public List<PrescriptionDetail> PrescriptionDetails { get; set; }
        public List<FoodUsageDetail> FoodUsageDetails { get; set; }
    }


}
