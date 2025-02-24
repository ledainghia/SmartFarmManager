using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.VaccineScheduleLog
{
    public class CreateVaccineLogRequest
    {
        public DateTime Date { get; set; }
        public int Session { get; set; }
        public Guid VaccineId { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
        public string Photo { get; set; }
        public Guid? TaskId { get; set; }
    }
}
