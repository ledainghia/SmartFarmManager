namespace SmartFarmManager.API.Payloads.Requests.Symptom
{
    public class UpdateSymptomRequest
    {
        public Guid? Id { get; set; }
        public string SymptomName { get; set; }
    }
}
