using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.LogInTask
{
    public class VaccineScheduleLogInTaskModel
    {
        public Guid ScheduleId { get; set; }
        public string GrowthStageName { get; set; }
        public string VaccineName { get; set; }
        public DateTime? Date { get; set; }
        public int? Quantity { get; set; }
        public int? ApplicationAge { get; set; }
        public decimal? TotalPrice { get; set; }
        public int Session { get; set; }
        

        public DateTime LogTime { get; set; }

        public string Notes { get; set; }

        public string? Photo { get; set; }

        public Guid? TaskId { get; set; }
    }
    
}
