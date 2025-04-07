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
        public DateTime? EndDate { get; set; }
        public string? Symptoms { get; set; }
        public List<PrescriptionMedicationResponse> Medications { get; set; }
    }

    public class PrescriptionMedicationResponse
    {
        public Guid MedicationId { get; set; }
        public string MedicationName { get; set; }
        public int Morning { get; set; }
        public int Afternoon { get; set; }
        public int Evening { get; set; }
        public int Noon { get; set; }
        public string? Notes { get; set; }
    }
}
