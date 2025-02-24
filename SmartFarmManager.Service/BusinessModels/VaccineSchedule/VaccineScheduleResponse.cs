using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.VaccineSchedule
{
    public class VaccineScheduleResponse
    {
        public Guid VaccineScheduleId { get; set; }
        public Guid StageId { get; set; }
        public Guid VaccineId { get; set; }
        public string VaccineName { get; set; } // Thêm Vaccine Name
        public DateTime? Date { get; set; }
        public int? Quantity { get; set; }
        public int? ApplicationAge { get; set; }
        public decimal? TotalPrice { get; set; }
        public int Session { get; set; }
        public string Status { get; set; }
    }

}
