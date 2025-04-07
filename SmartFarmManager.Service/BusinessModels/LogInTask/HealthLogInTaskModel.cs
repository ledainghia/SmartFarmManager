using SmartFarmManager.Service.BusinessModels.Prescription;
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
        public  PrescriptionInHealthLogModel Prescription { get; set; }
    }

    public class PrescriptionInHealthLogModel
    {
        public string Notes { get; set; }
        public DateTime? PrescribedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int QuantityAnimal { get; set; }
        public int? RemainingQuantity { get; set; }
        public string Status { get; set; }
        public int? DaysToTake { get; set; }
        public decimal? Price { get; set; }
        public List<PrescriptionMedicationInHealthLogModel> PrescriptionMedications { get; set; } = new List<PrescriptionMedicationInHealthLogModel>();
    }
    public class PrescriptionMedicationInHealthLogModel
    {
        public Guid MedicationId { get; set; }    
        public string Notes { get; set; }
        public int Morning { get; set; } = 0;
        public int Afternoon { get; set; } = 0;
        public int Evening { get; set; } = 0;
        public int Noon { get; set; } = 0;
        public MedicationInHealthLogModel Medication { get; set; }
    }
    public class MedicationInHealthLogModel
    {
        public string Name { get; set; }

        public string UsageInstructions { get; set; }

        public decimal? Price { get; set; }

        public int? DoseWeight { get; set; }

        public int? Weight { get; set; } 

        public int? DoseQuantity { get; set; } 

        public decimal? PricePerDose { get; set; }
    }

}
