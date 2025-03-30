using SmartFarmManager.Service.BusinessModels.StandardPrescription;

namespace SmartFarmManager.API.Payloads.Requests.StandardPrescription
{
    public class UpdateStandardPrescriptionRequest
    {
        public string? Notes { get; set; }
        public int? RecommendDay { get; set; }

        public List<CreateStandardPrescriptionMedicationModel>? StandardPrescriptionMedications { get; set; }

        public UpdateStandardPrescriptionModel MapToModel()
        {
            return new UpdateStandardPrescriptionModel
            {
                Notes = this.Notes,
                RecommendDay = this.RecommendDay,
                StandardPrescriptionMedications = this.StandardPrescriptionMedications?.Select(m => new CreateStandardPrescriptionMedicationModel
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
