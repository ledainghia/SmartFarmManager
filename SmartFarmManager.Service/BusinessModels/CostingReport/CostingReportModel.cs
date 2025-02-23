using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.CostingReport
{
    public class CostingReportModel
    {
        public Guid Id { get; set; }
        public int ReportMonth { get; set; }
        public int ReportYear { get; set; }
        public string CostType { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
