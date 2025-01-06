using SmartFarmManager.API.Payloads.Responses.Picture;
using SmartFarmManager.API.Payloads.Responses.Prescription;
using SmartFarmManager.Service.BusinessModels.Prescription;

namespace SmartFarmManager.API.Payloads.Responses.MedicalSymptom
{
    public class MedicalSymptomResponse
    {
        public Guid Id { get; set; }
        public Guid FarmingBatchId { get; set; }
        public string Symptoms { get; set; }
        public string Diagnosis { get; set; }
        public DateTime? CreateAt { get; set; }
        public string Status { get; set; }
        public int? AffectedQuantity { get; set; }
        public int? Quantity { get; set; }
        public string NameAnimal { get; set; }
        public string Notes { get; set; }
        public List<PictureResponse> Pictures { get; set; }
        public List<PrescriptionResponse> Prescriptions { get; set; }

    }
}
