using SmartFarmManager.Service.BusinessModels.StandardPrescription;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.StandardPrescription
{
    public class CreateStandardPrescriptionRequest
    {
        [Required]
        public Guid DiseaseId { get; set; }
        [Required]
        public string Notes { get; set; }
        [Required]
        public int RecommendDay { get; set; }
        public List<CreateStandardPrescriptionMedicationModel> StandardPrescriptionMedications { get; set; }
        public CreateStandardPrescriptionModel MapToModel()
        {
            return new CreateStandardPrescriptionModel
            {
                DiseaseId = this.DiseaseId,
                Notes = this.Notes,
                RecommendDay = this.RecommendDay,
                StandardPrescriptionMedications = this.StandardPrescriptionMedications.Select(m => new CreateStandardPrescriptionMedicationModel
                {
                    MedicationId = m.MedicationId,
                    Morning = m.Morning,
                    Noon = m.Noon,
                    Afternoon = m.Afternoon,
                    Evening = m.Evening
                }).ToList()
            };
        }
    }
}
