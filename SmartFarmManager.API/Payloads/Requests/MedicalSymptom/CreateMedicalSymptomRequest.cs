using SmartFarmManager.API.Payloads.Requests.Picture;

namespace SmartFarmManager.API.Payloads.Requests.MedicalSymptom
{
    public class CreateMedicalSymptomRequest
    {
        public Guid FarmingBatchId { get; set; }
        public string Symptoms { get; set; }
        public string Status { get; set; }
        public int? AffectedQuantity { get; set; }
        public string Notes { get; set; }
        public List<PictureRequest> Pictures { get; set; } = new List<PictureRequest>();
    }
}
