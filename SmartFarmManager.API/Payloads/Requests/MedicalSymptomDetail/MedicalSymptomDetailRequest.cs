namespace SmartFarmManager.API.Payloads.Requests.MedicalSymptomDetail
{
    public class MedicalSymptomDetailRequest
    {
        public Guid SymptomId { get; set; }
        public string? Notes { get; set; }
    }
}
