using SmartFarmManager.Service.BusinessModels.Prescription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.HealthLog
{
    public class HealthLogModel
    {
        public Guid Id { get; set; }
        public Guid? PrescriptionId { get; set; }
        public DateTime? Date { get; set; }
        public string Notes { get; set; }
        public string Photo { get; set; }
        public PrescriptionModel? Prescriptions { get; set; }
        public Guid? TaskId { get; set; }
    }
}
