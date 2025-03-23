using SmartFarmManager.Service.BusinessModels.Medication;

namespace SmartFarmManager.API.Payloads.Requests.Medication
{
    public class UpdateMedicationRequest
    {
        public string? Name { get; set; }
        public string? UsageInstructions { get; set; }
        public decimal? Price { get; set; }
        public int? DoseWeight { get; set; }
        public int? Weight { get; set; }
        public int? DoseQuantity { get; set; }
        public decimal? PricePerDose { get; set; }

        public UpdateMedicationModel MapToModel()
        {
            return new UpdateMedicationModel
            {
                Name = this.Name,
                UsageInstructions = this.UsageInstructions,
                Price = this.Price,
                DoseWeight = this.DoseWeight,
                Weight = this.Weight,
                DoseQuantity = this.DoseQuantity,
                PricePerDose = this.PricePerDose
            };
        }
    }

}
