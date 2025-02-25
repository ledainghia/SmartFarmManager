using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.GrowthStage
{
    public class UpdateGrowthStageRequest
    {
        public Guid GrowthStageId { get; set; }
        public decimal WeightAnimal { get; set; }
    }

}
