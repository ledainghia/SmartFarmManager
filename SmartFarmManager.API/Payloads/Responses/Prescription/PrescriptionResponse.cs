namespace SmartFarmManager.API.Payloads.Responses.Prescription
{
    public class PrescriptionResponse
    {
        public Guid Id { get; set; }
        public DateTime? PrescribedDate { get; set; }
        public string? Notes { get; set; }
        public int? QuantityAnimal { get; set; }
        public string? Status { get; set; }
        public decimal? Price { get; set; }
        public Guid? CageId { get; set; }
        public int? DaysToTake { get; set; }
        public List<PrescriptionMedicationResponse> Medications { get; set; }
    }

    public class PrescriptionMedicationResponse
    {
        public Guid MedicationId { get; set; }
        public string MedicationName { get; set; }
        public int Dosage { get; set; }
        public int Duration { get; set; }
        public bool Morning { get; set; }
        public bool Afternoon { get; set; }
        public bool Evening { get; set; }
        public bool Noon { get; set; }
    }
}
