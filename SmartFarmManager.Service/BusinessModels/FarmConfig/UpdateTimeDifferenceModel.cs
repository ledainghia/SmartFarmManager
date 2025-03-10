using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.FarmConfig
{
   public class UpdateTimeDifferenceModel
    {
        public Guid FarmId { get; set; }
        public DateTime NewTime { get; set; }
    }
}
