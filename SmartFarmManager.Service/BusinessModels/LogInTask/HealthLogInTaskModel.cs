using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.LogInTask
{
    public class HealthLogInTaskModel
    {
        public Guid PrescriptionId { get; set; }
        public string Notes { get; set; }
        public string Photo { get; set; }
        public Guid? TaskId { get; set; }
        public DateTime LogTime { get; set; } 
    }
}
