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
        public int Dosage { get; set; }
        public bool Morning { get; set; }
        public bool Afternoon { get; set; }
        public bool Evening { get; set; }
        public bool Noon { get; set; }
        public MedicationModel Medication { get; set; }
    }
}
