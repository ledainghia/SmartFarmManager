namespace SmartFarmManager.API.Payloads.Requests.Prescription
{
    public class CreateNewPrescriptionRequest
    {
        public Guid RecordId { get; set; } // ID triệu chứng
        public Guid CageId { get; set; }
        public DateTime PrescribedDate { get; set; }
        public string Notes { get; set; }
        public int QuantityAnimal { get; set; }
        public string Status { get; set; }
        public int DaysToTake { get; set; }
        public string Disease { get; set; }
        public string CageAnimalName { get; set; }
        public string Symptoms { get; set; }
        public List<PrescriptionMedicationRequest> Medications { get; set; }
    }
}
