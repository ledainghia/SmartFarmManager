namespace SmartFarmManager.API.Payloads.Requests.Prescription
{
    public class UpdatePrescriptionRequest
    {
        public Guid Id { get; set; }
        public DateTime? PrescribedDate { get; set; }
        public string? CaseType { get; set; }
        public string? Notes { get; set; }
        public int? QuantityAnimal { get; set; }
        public string? Status { get; set; }
        public decimal? Price { get; set; }
        public Guid? CageId { get; set; }
        public int? DaysToTake { get; set; }
    }
}
