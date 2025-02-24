using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.FarmingBatch
{
    public class FarmingBatchReportResponse
    {
        public Guid FarmingBatchId { get; set; }
        public string FarmingBatchName { get; set; }

        public string CageName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? TotalEggSales { get; set; }      // Tổng tiền bán trứng
        public decimal? TotalMeatSales { get; set; }     // Tổng tiền bán thịt
        public decimal? TotalFoodCost { get; set; }      // Tổng tiền thức ăn
        public decimal? TotalVaccineCost { get; set; }   // Tổng tiền vaccine
        public decimal? TotalMedicineCost { get; set; }  // Tổng tiền thuốc
        public decimal? NetProfit { get; set; }          // Lợi nhuận (Tổng tiền bán - Tổng chi phí)
    }

}
