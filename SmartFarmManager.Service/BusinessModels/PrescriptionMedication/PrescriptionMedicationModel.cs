using SmartFarmManager.Service.BusinessModels.Medication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.PrescriptionMedication
{
    public class PrescriptionMedicationModel
    {
        public Guid MedicationId { get; set; }
        public int Morning { get; set; }
        public int Afternoon { get; set; }
        public int Evening { get; set; }
        public int Noon { get; set; }
        public string? Notes { get; set; }
        public MedicationModel Medication { get; set; }
    }
}
