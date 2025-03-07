using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.FarmConfig
{
    public class FarmConfigItemModel
    {
        public Guid FarmId { get; set; }
        public int MaxCagesPerStaff { get; set; }
        public int MaxFarmingBatchesPerCage { get; set; }
    }
}
