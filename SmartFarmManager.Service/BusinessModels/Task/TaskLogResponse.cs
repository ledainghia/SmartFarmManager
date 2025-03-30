using SmartFarmManager.Service.BusinessModels.DailyFoodUsageLog;
using SmartFarmManager.Service.BusinessModels.HealthLog;
using SmartFarmManager.Service.BusinessModels.VaccineScheduleLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class TaskLogResponse
    {
        public DailyFoodUsageLogModel? FoodLog { get; set; }
        public VaccineScheduleLogModel? VaccineLog { get; set; }
        public HealthLogModel? HealthLog { get; set; }
    }

}
