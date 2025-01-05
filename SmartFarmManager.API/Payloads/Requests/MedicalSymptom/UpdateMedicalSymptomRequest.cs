namespace SmartFarmManager.API.Payloads.Requests.MedicalSymptom
{
    public class UpdateMedicalSymptomRequest
    {
        public string Diagnosis { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

}
