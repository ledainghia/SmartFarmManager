using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.VaccineSchedule
{
    public class VaccineScheduleItemModel
    {
        public Guid Id { get; set; }
        public Guid VaccineId { get; set; }
        public string VaccineName { get; set; }
        public Guid StageId { get; set; }
        public string StageName { get; set; }
        public Guid FarmingBatchId { get; set; }
        public string FarmingBatchName { get; set; }
        public Guid CageId { get; set; }
        public string CageName { get; set; }
        public DateTime? Date { get; set; }
        public int? Quantity { get; set; }
        public int? ApplicationAge { get; set; }
        public decimal? ToltalPrice { get; set; }
        public int Session { get; set; }
        public string Status { get; set; }
    }
}
