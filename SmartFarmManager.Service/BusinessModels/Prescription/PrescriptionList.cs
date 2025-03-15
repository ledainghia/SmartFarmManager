using SmartFarmManager.Service.BusinessModels.PrescriptionMedication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Prescription
{
    public class PrescriptionList
    {
        public Guid Id { get; set; }
        public Guid? CageId { get; set; }
        public Guid? RecordId { get; set; }
        public DateTime? PrescribedDate { get; set; }
        public int? QuantityAnimal { get; set; }
        public int? RemainingQuantity { get; set; }
        public string? CaseType { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        public decimal? Price { get; set; }
        public int? DaysToTake { get; set; }
        public DateTime? EndDate { get; set; }
        public string Disease { get; set; }
        public string? CageAnimalName { get; set; }
        public string Symptoms { get; set; }
        public int? QuantityInCage { get; set; }
        public List<PrescriptionMedicationModel> Medications { get; set; }
    }
}
