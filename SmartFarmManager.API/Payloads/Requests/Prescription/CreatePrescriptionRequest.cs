namespace SmartFarmManager.API.Payloads.Requests.Prescription
{
    public class CreatePrescriptionRequest
    {
        public Guid? MedicalSymptomId { get; set; }
        public DateTime PrescribedDate { get; set; }
        public string Notes { get; set; }
        public Guid? CageId { get; set; }
        public int DaysToTake { get; set; }
        public int QuantityAnimal { get; set; }
        public string Status { get; set; }
        public List<PrescriptionMedicationRequest> Medications { get; set; }
        
    }
    public class PrescriptionMedicationRequest
    {
        public Guid MedicationId { get; set; }
        public int Morning { get; set; }
        public int Afternoon { get; set; }
        public int Evening { get; set; }
        public int Noon { get; set; }
    }
}
