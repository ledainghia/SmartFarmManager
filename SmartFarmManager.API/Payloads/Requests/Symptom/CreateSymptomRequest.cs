namespace SmartFarmManager.API.Payloads.Requests.Symptom
{
    public class CreateSymptomRequest
    {
        public Guid? Id { get; set; }
        public string SymptomName { get; set; }
    }
}
