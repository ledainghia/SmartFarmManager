using SmartFarmManager.Service.BusinessModels.ElectricityLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.WaterLog
{
    public class WaterLogInMonthModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalConsumptionInMonth { get; set; }
        public decimal TotalConsumptionInDateAverage { get; set; }
        public List<WaterLogInDayModel> Records { get; set; }
    }
}
