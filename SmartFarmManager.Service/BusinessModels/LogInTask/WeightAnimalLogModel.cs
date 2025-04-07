using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Log
{
    public class WeightAnimalLogModel
    {
        public Guid GrowthStageId { get; set; }
        public decimal? OldWeight { get; set; }
        public decimal? NewWeight { get; set; }
        public DateTime LogTime { get; set; }
        public Guid TaskId { get; set; }
    }
}
