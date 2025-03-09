using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.Webhook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.ElectricityLog
{
    public class ElectricityLogInDayModel
    {
        public Guid FarmId { get; set; }
        public List<ElectricRecordModel> Data { get; set; }
        public decimal TotalConsumption { get; set; }
    }
}
