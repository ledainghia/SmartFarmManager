using SmartFarmManager.API.Payloads.Responses.Picture;

namespace SmartFarmManager.API.Payloads.Responses.MedicalSymptom
{
    public class MedicalSymptomResponse
    {
        public Guid Id { get; set; }
        public Guid FarmingBatchId { get; set; }
        public string Symptoms { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string Status { get; set; }
        public int? AffectedQuantity { get; set; }
        public int? Quantity { get; set; }
        public string NameAnimal { get; set; }
        public string Notes { get; set; }
        public List<PictureResponse> Pictures { get; set; } = new List<PictureResponse>();
    }
}
