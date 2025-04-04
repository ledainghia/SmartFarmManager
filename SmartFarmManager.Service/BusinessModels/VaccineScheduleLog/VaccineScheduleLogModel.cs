using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.VaccineScheduleLog
{
    public class VaccineScheduleLogModel
    {
        public Guid Id { get; set; }
        public Guid? ScheduleId { get; set; }
        public string? VaccineName { get; set; }
        public DateOnly? Date { get; set; }
        public string Notes { get; set; }
        public string Photo { get; set; }
        public Guid? TaskId { get; set; }
        public int? Quantity { get; set; }
        public decimal? ToltalPrice { get; set; }
    }
}
