using SmartFarmManager.Service.BusinessModels.AnimalSale;
using SmartFarmManager.Service.BusinessModels.DailyFoodUsageLog;
using SmartFarmManager.Service.BusinessModels.HealthLog;
using SmartFarmManager.Service.BusinessModels.Log;
using SmartFarmManager.Service.BusinessModels.LogInTask;
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
        public DailyFoodUsageLogInTaskModel? FoodLog { get; set; }
        public VaccineScheduleLogInTaskModel? VaccineLog { get; set; }
        public HealthLogInTaskModel? HealthLog { get; set; }
        public WeightAnimalLogModel? WeightLog { get; set; }
        public AnimalSaleLogByTaskModel? SaleLog { get; set; } 
        public MedicalSymptomLogInTaskModel? MedicalSymptomLog { get; set; }
    }

}
