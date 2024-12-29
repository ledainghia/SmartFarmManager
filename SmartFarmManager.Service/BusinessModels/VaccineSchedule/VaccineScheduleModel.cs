using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.VaccineSchedule
{
    public class VaccineScheduleModel
    {
        public Guid Id { get; set; }
        public Guid StageId { get; set; }
        public Guid VaccineId { get; set; }
        public DateTime? Date { get; set; }
        public int? ApplicationAge { get; set; }
        public int? Quantity { get; set; }
        public string Status { get; set; }
    }
}
