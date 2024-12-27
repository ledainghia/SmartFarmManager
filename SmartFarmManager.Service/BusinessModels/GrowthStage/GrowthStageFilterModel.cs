using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.GrowthStage
{
    public class GrowthStageFilterModel
    {
        public Guid? FarmingBatchId { get; set; }
        public string? Name { get; set; }
        public int? AgeStart { get; set; }
        public int? AgeEnd { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string OrderBy { get; set; } 
        public string Order { get; set; }
    }

}
