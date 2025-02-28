using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.EggHarvest
{
    public class CreateEggHarvestRequest
    {
        public Guid GrowthStageId { get; set; }
        public int EggCount { get; set; }
        public string? Notes { get; set; }
        public Guid TaskId { get; set; }
    }
}
