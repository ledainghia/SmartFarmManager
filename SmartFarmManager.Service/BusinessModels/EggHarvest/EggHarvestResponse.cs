using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.EggHarvest
{
    public class EggHarvestResponse
    {
        public Guid Id { get; set; }
        public Guid GrowthStageId { get; set; }
        public string GrowthStageName { get; set; }
        public DateTime DateCollected { get; set; }
        public int EggCount { get; set; }
        public string? Notes { get; set; }
        public Guid TaskId { get; set; }
    }
}
