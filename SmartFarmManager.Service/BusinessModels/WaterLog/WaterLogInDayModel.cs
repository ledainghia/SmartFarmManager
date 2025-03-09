using SmartFarmManager.Service.BusinessModels.Webhook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.WaterLog
{
    public class WaterLogInDayModel
    {
        public Guid FarmId { get; set; }
        public List<WaterRecordModel> Data { get; set; }
        public decimal TotalConsumption { get; set; }
    }
}
