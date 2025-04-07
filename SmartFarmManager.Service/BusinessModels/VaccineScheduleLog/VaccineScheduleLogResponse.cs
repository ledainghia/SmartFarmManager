using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.VaccineScheduleLog
{
    public class VaccineScheduleLogResponse
    {
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public DateOnly? Date { get; set; }
        public string Notes { get; set; }
        public string? Photo { get; set; }
        public Guid? TaskId { get; set; }
    }
}
