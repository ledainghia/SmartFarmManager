﻿namespace SmartFarmManager.API.Payloads.Requests.Prescription
{
    public class CreatePrescriptionRequest
    {
        public Guid RecordId { get; set; }
        public DateTime PrescribedDate { get; set; }
        public string CaseType { get; set; }
        public string Notes { get; set; }
        public Guid CageId { get; set; }
        public int DaysToTake { get; set; }
        public int QuantityAnimal { get; set; }
        public List<PrescriptionMedicationRequest> Medications { get; set; }
        public string Status { get; set; }
    }
    public class PrescriptionMedicationRequest
    {
        public Guid MedicationId { get; set; }
        public int Dosage { get; set; }
        public bool Morning { get; set; }
        public bool Afternoon { get; set; }
        public bool Evening { get; set; }
        public bool Night { get; set; }
    }
}
